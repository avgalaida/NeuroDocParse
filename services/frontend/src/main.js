import { createApp, h, provide } from 'vue';
import { ApolloClient, InMemoryCache, HttpLink, split, ApolloLink } from '@apollo/client/core';
import { createApolloProvider } from '@vue/apollo-option';
import { DefaultApolloClient } from '@vue/apollo-composable';
import { WebSocketLink } from '@apollo/client/link/ws';
import { getMainDefinition } from '@apollo/client/utilities';
import App from './App.vue';
import keycloak from './keycloak';
import { createStore } from 'vuex';

const store = createStore({
  state: {
    imageData: null,
  },
  mutations: {
    setImageData(state, data) {
      state.imageData = data;
    },
    clearImageData(state) {
      state.imageData = null;
    },
  },
  getters: {
    imageData: (state) => state.imageData,
  },
  actions: {
    setImageData({ commit }, data) {
      commit('setImageData', data);
    },
    clearImageData({ commit }) {
      commit('clearImageData');
    },
  },
});

const httpLink = new HttpLink({
  uri: 'http://127.0.0.1:5050/graphql',
});

const wsLink = new WebSocketLink({
  uri: 'ws://127.0.0.1:5050/graphql',
  options: {
    reconnect: true,
  },
});

const authLink = new ApolloLink((operation, forward) => {
  const token = keycloak.token ? `Bearer ${keycloak.token}` : '';
  operation.setContext({
    headers: {
      Authorization: token,
    },
  });
  return forward(operation);
});

const splitLink = split(
  ({ query }) => {
    const definition = getMainDefinition(query);
    return (
      definition.kind === 'OperationDefinition' &&
      definition.operation === 'subscription'
    );
  },
  wsLink,
  authLink.concat(httpLink),
);

const apolloClient = new ApolloClient({
  link: splitLink,
  cache: new InMemoryCache(),
});

const apolloProvider = createApolloProvider({
  defaultClient: apolloClient,
});

keycloak.init({ 
  onLoad: 'login-required',
  checkLoginIframe: false,
  silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html'
}).then(authenticated => {
  if (authenticated) {
    const app = createApp({
      setup() {
        provide(DefaultApolloClient, apolloClient);
        provide('keycloak', keycloak);
      },
      render: () => h(App),
    });
    app.use(apolloProvider);
    app.use(store); // Добавление Vuex
    app.config.globalProperties.$keycloak = keycloak;
    app.mount('#app');
  } else {
    console.log('Authentication failed');
    window.location.reload();
  }
}).catch(error => {
  console.error('Keycloak initialization failed:', error);
});
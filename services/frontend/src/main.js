import { createApp, h, provide } from 'vue';
import { ApolloClient, InMemoryCache, HttpLink, split, ApolloLink } from '@apollo/client/core';
import { createApolloProvider } from '@vue/apollo-option';
import { DefaultApolloClient } from '@vue/apollo-composable';
import { WebSocketLink } from '@apollo/client/link/ws';
import { getMainDefinition } from '@apollo/client/utilities';
import App from './App.vue';
import keycloak from './keycloak';

// Настройка Apollo клиента
const httpLink = new HttpLink({
  uri: process.env.VUE_APP_GRAPHQL_HTTP,
});

const wsLink = new WebSocketLink({
  uri: process.env.VUE_APP_GRAPHQL_WS,
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

// Создание Apollo провайдера
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
        provide('keycloak', keycloak); // Provide keycloak instance to the app
      },
      render: () => h(App),
    });
    app.use(apolloProvider);
    app.config.globalProperties.$keycloak = keycloak; // Add keycloak to global properties
    app.mount('#app');
  } else {
    console.log('Authentication failed');
    window.location.reload();
  }
}).catch(error => {
  console.error('Keycloak initialization failed:', error);
});
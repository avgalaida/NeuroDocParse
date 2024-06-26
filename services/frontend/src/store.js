import { createStore } from 'vuex';

export default createStore({
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
  actions: {
    setImageData({ commit }, data) {
      commit('setImageData', data);
    },
    clearImageData({ commit }) {
      commit('clearImageData');
    },
  },
  getters: {
    imageData: (state) => state.imageData,
  },
});
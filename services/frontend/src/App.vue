<template>
  <div id="app">
    <header>
      Система извлечения данных из шаблонных документов
    </header>
    <div class="container">
      <aside>
        <nav>
          <ul>
            <li :class="{ active: currentView === 'extractData' }" @click="setView('extractData')">Извлечение данных</li>
            <li :class="{ active: currentView === 'selectDocumentClass' }" @click="setView('selectDocumentClass')">Выбор класса документа</li>
            <li :class="{ active: currentView === 'documentMarkup' }" @click="setView('documentMarkup')">Разметка документа</li>
          </ul>
        </nav>
      </aside>
      <main>
        <component :is="currentViewComponent" @document-received="handleDocumentReceived" :document="document" />
      </main>
    </div>
  </div>
</template>

<script>
import ExtractData from './components/ExtractData.vue';
import SelectDocumentClass from './components/SelectDocumentClass.vue';
import DocumentMarkup from './components/DocumentMarkup.vue';
import DocumentTemplate from './components/DocumentTemplate.vue';

export default {
  name: 'App',
  data() {
    return {
      currentView: 'extractData',
      document: null,
    };
  },
  computed: {
    currentViewComponent() {
      switch (this.currentView) {
        case 'extractData':
          return ExtractData;
        case 'selectDocumentClass':
          return SelectDocumentClass;
        case 'documentMarkup':
          return DocumentMarkup;
        case 'documentTemplate':
          return DocumentTemplate;
        default:
          return ExtractData;
      }
    },
  },
  methods: {
    setView(view) {
      this.currentView = view;
    },
    handleDocumentReceived(document) {
      this.document = document;
      this.setView('documentTemplate'); // Перенаправляем на страницу шаблона документа
    },
  },
};
</script>

<style>
#app {
  font-family: Arial, sans-serif;
}

header {
  background-color: #4CAF50;
  color: white;
  text-align: center;
  padding: 10px 0;
  font-size: 24px;
}

.container {
  display: flex;
}

aside {
  width: 200px;
  background-color: #f4f4f4;
  padding: 15px;
}

nav ul {
  list-style-type: none;
  padding: 0;
}

nav li {
  padding: 10px;
  cursor: pointer;
}

nav li.active {
  background-color: #ddd;
  font-weight: bold;
}

main {
  flex-grow: 1;
  padding: 20px;
}
</style>
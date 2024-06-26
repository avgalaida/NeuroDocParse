<template>
  <div class="document-template">
    <div class="document-info">
      <h2>Распознанный документ: {{ document?.DocumentName || 'Неизвестный документ' }}</h2>
      <form v-if="localDocument && templateFields.length > 0">
        <div v-for="field in templateFields" :key="field.field" class="field-group">
          <label :for="field.field">{{ field.label }}:</label>
          <input
            v-model="localDocument.Fields[field.field]"
            :id="field.field"
            :placeholder="field.placeholder"
            :class="{ 'error': !localDocument.Fields[field.field] }"
          />
          <span v-if="!localDocument.Fields[field.field]" class="error-message">Ошибка обнаружения</span>
        </div>
        <button type="button" @click="downloadJson">Скачать JSON</button>
      </form>
      <div v-else>
        <p>Данные документа отсутствуют.</p>
      </div>
    </div>
    <div class="document-image-container">
      <image-editor 
        v-if="localDocument.Base64Image" 
        :image-src="'data:image/png;base64,' + localDocument.Base64Image" 
        @update:image="updateImage" 
        :user-id="userId" 
      />
    </div>
  </div>
</template>

<script>
import { documentTemplates } from '../documentTemplates';
import ImageEditor from './ImageEditor.vue'; // убедитесь, что путь правильный

export default {
  name: 'DocumentTemplate',
  components: {
    ImageEditor,
  },
  props: ['document', 'userId'],
  data() {
    return {
      localDocument: this.convertDocument(this.document),
      templateFields: [],
    };
  },
  watch: {
    document: {
      handler(newValue) {
        this.localDocument = this.convertDocument(newValue);
        this.templateFields = documentTemplates[this.localDocument.DocumentName?.toLowerCase()] || [];
      },
      deep: true,
      immediate: true,
    },
  },
  methods: {
    convertDocument(doc) {
      if (!doc) {
        console.error('Document is null or undefined');
        return { Fields: {} };
      }
      if (!doc.Fields) {
        console.error('Fields property is missing in document', doc);
        return { Fields: {} };
      }

      let convertedDoc = JSON.parse(JSON.stringify(doc));
      for (let key in convertedDoc.Fields) {
        if (Array.isArray(convertedDoc.Fields[key])) {
          convertedDoc.Fields[key] = convertedDoc.Fields[key][0];
        }
      }
      return convertedDoc;
    },
    downloadJson() {
      const jsonData = {
        _type: this.localDocument.DocumentName,
      };

      for (const field of this.templateFields) {
        jsonData[field.field] = this.localDocument.Fields[field.field] || '';
      }

      const jsonString = JSON.stringify(jsonData, null, 2);
      const blob = new Blob([jsonString], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'document.json';
      a.click();
      URL.revokeObjectURL(url);
    },
    updateImage(base64Image) {
      this.localDocument.Base64Image = base64Image;
      this.$emit('update:image', base64Image);
    },
  },
};
</script>

<style scoped>
.document-template {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.document-info {
  flex: 1;
  padding-right: 20px;
}

.document-image-container {
  flex: 0 0 40%; /* Ширина контейнера для изображения */
  text-align: center;
}

.document-image {
  max-width: 100%;
  height: auto;
}

.field-group {
  margin-bottom: 15px;
}

label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

input {
  width: 100%;
  padding: 8px;
  box-sizing: border-box;
}

.error {
  border: 1px solid red;
}

.error-message {
  color: red;
  font-size: 12px;
  margin-top: 5px;
}

button {
  margin-top: 20px;
  padding: 10px 20px;
  background-color: #4CAF50;
  color: white;
  border: none;
  cursor: pointer;
}

button:hover {
  background-color: #45a049;
}
</style>
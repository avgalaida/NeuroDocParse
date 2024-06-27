<template>
  <div class="document-class-selector">
    <h2>Выбор класса документа</h2>
    <form @submit.prevent="uploadFile">
      <label for="documentClass">Класс документа:</label>
      <select v-model="selectedClass" id="documentClass" required>
        <option value="passport">Паспорт</option>
        <option value="snils">СНИЛС</option>
        <option value="drive_id">Водительское удостоверение</option>
      </select>
      <div v-if="imageSrc">
        <image-editor-select
          :image-src="imageSrc"
          @update:image="updateImage"
          ref="imageEditorSelect"
        />
      </div>
      <input type="file" @change="onFileChange" ref="fileInput" style="display: none" />
      <button type="button" @click="triggerFileInput">Загрузить изображение</button>
      <button type="submit" :disabled="!imageSrc">Отправить на обработку</button>
    </form>
    <div v-if="loading">Загрузка...</div>
    <div v-if="error">Ошибка: {{ error.message }}</div>
    <document-template v-if="document" :document="document" :userId="userId" />
  </div>
</template>

<script>
import ImageEditorSelect from './ImageEditorSelect.vue';
import DocumentTemplate from './DocumentTemplate.vue';
import { mapActions } from 'vuex';
import { UPLOAD_IMAGE_MUTATION, DATA_EXTRACTED_SUBSCRIPTION } from '@/graphql';
import { v4 as uuidv4 } from 'uuid';

export default {
  name: 'SelectDocumentClass',
  components: {
    ImageEditorSelect,
    DocumentTemplate,
  },
  data() {
    return {
      selectedClass: '',
      imageSrc: null,
      b64Img: null,
      loading: false,
      error: null,
      userId: null,
      requestId: uuidv4(),
      document: null,
    };
  },
  mounted() {
    const keycloak = this.$keycloak;
    if (keycloak && keycloak.tokenParsed) {
      this.userId = keycloak.tokenParsed.sub;
    } else {
      console.error('Keycloak token is not available.');
    }

    this.subscribeToDataExtracted();
  },
  methods: {
    ...mapActions(['setImageData']),
    triggerFileInput() {
      this.$refs.fileInput.click();
    },
    onFileChange(event) {
      const files = event.target.files || event.dataTransfer.files;
      if (!files.length) return;
      const file = files[0];
      const reader = new FileReader();
      reader.onload = () => {
        this.imageSrc = reader.result;
        this.b64Img = reader.result.split(',')[1];
      };
      reader.readAsDataURL(file);
    },
    async uploadFile() {
      if (!this.b64Img || !this.selectedClass || !this.userId) return;
      this.loading = true;
      this.error = null;

      try {
        const editedImage = await this.$refs.imageEditorSelect.getCroppedImage();
        await this.$apollo.mutate({
          mutation: UPLOAD_IMAGE_MUTATION,
          variables: {
            b64Img: editedImage,
            userId: this.userId,
            requestId: this.requestId,
            requestType: 'duo',
            model: this.selectedClass,
          },
        });
        this.$emit('document-uploaded');
      } catch (error) {
        this.error = error;
      } finally {
        this.loading = false;
      }
    },
    updateImage(base64Image) {
      this.b64Img = base64Image;
    },
    subscribeToDataExtracted() {
      this.$apollo.subscribe({
        query: DATA_EXTRACTED_SUBSCRIPTION,
        variables: { requestId: this.requestId },
      }).subscribe({
        next: ({ data }) => {
          this.document = data.dataExtracted;
          this.setImageData(data.dataExtracted);
        },
        error: (err) => console.error('Subscription error:', err),
      });
    },
  },
};
</script>

<style scoped>
.document-class-selector {
  text-align: center;
}

form {
  display: inline-block;
  text-align: left;
}

label, select, button {
  display: block;
  margin-bottom: 10px;
}

button {
  padding: 10px 20px;
  background-color: #4CAF50;
  color: white;
  border: none;
  cursor: pointer;
}

button:hover {
  background-color: #45a049;
}

button:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}
</style>
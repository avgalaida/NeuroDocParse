<template>
  <div class="upload-container" @drop.prevent="onDrop" @dragover.prevent>
    <input type="file" @change="onFileChange" ref="fileInput" style="display: none" />
    <button @click="triggerFileInput">Загрузить изображение или перетащить сюда</button>
    <div v-if="loading">Загрузка...</div>
    <div v-if="error">Ошибка: {{ error.message }}</div>
    <div v-if="data">
      <DocumentTemplate :document="data.uploadImage" ref="documentTemplate" @update:image="uploadEditedImage" />
    </div>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex';
import { UPLOAD_IMAGE_MUTATION, DATA_EXTRACTED_SUBSCRIPTION } from '@/graphql';
import DocumentTemplate from './DocumentTemplate.vue';

export default {
  name: 'ExtractData',
  components: {
    DocumentTemplate,
  },
  data() {
    return {
      loading: false,
      error: null,
      data: null,
      b64Img: null,
      userId: null,
    };
  },
  computed: {
    ...mapGetters(['imageData']),
  },
  watch: {
    imageData(newData) {
      if (newData) {
        this.data = { uploadImage: newData };
      } else {
        this.data = null;
      }
    },
  },
  mounted() {
    this.resetState();
    const keycloak = this.$keycloak;
    console.log('Keycloak instance:', keycloak);
    if (keycloak && keycloak.tokenParsed) {
      this.userId = keycloak.tokenParsed.sub;
      console.log('User ID:', this.userId);
    } else {
      console.error('Keycloak token is not available.');
    }

    this.$apollo.subscribe({
      query: DATA_EXTRACTED_SUBSCRIPTION,
    }).subscribe({
      next: ({ data }) => {
        console.log('Subscription data:', data);
        this.setImageData(data.dataExtracted);

        if (data.dataExtracted.Base64Image && this.$refs.documentTemplate) {
          this.$refs.documentTemplate.updateImage(data.dataExtracted.Base64Image);
        }
      },
      error: (err) => console.error('Subscription error:', err),
    });
  },
  beforeUnmount() {
    this.resetState();
  },
  methods: {
    ...mapActions(['setImageData', 'clearImageData']),
    triggerFileInput() {
      this.$refs.fileInput.click();
    },
    onFileChange(event) {
      const files = event.target.files || event.dataTransfer.files;
      if (!files.length) return;
      const file = files[0];
      this.uploadFile(file);
    },
    onDrop(event) {
      const files = event.dataTransfer.files;
      if (!files.length) return;
      const file = files[0];
      this.uploadFile(file);
    },
    async uploadFile(file) {
      if (!file) return;
      if (!this.userId) {
        this.error = new Error('User ID is not available.');
        return;
      }

      this.resetState();  // Явный сброс состояния перед началом новой загрузки

      this.loading = true;

      try {
        const reader = new FileReader();
        reader.onload = async () => {
          this.b64Img = reader.result.split(',')[1];
          console.log('Image base64:', this.b64Img);
          console.log('User ID:', this.userId);

          // Очистка кэша Apollo Client
          await this.$apollo.provider.defaultClient.cache.reset();

          try {
            await this.$apollo.mutate({
              mutation: UPLOAD_IMAGE_MUTATION,
              variables: { b64Img: this.b64Img, userId: this.userId },
              fetchPolicy: 'no-cache',  // Отключаем кэширование Apollo Client
            });

            console.log('Image upload mutation successful, waiting for subscription data...');
            // Теперь ждем данные из подписки
          } catch (graphqlError) {
            this.error = graphqlError;
            console.error('GraphQL error:', graphqlError);
          } finally {
            this.loading = false;
          }

          this.b64Img = null;
        };
        reader.readAsDataURL(file);
      } catch (error) {
        this.loading = false;
        this.error = error;
        console.error('File upload failed:', error);
      }
    },
    resetState() {
      this.loading = false;
      this.error = null;
      this.data = null;
      this.b64Img = null;
      this.clearImageData();  // Очистка данных изображения в Vuex
      console.log('State reset');
    },
    async uploadEditedImage(base64Image) {
      if (!this.userId) {
        this.error = new Error('User ID is not available.');
        return;
      }

      this.resetState();  // Явный сброс состояния перед началом новой загрузки

      this.loading = true;

      try {
        // Очистка кэша Apollo Client
        await this.$apollo.provider.defaultClient.cache.reset();

        try {
          await this.$apollo.mutate({
            mutation: UPLOAD_IMAGE_MUTATION,
            variables: { b64Img: base64Image, userId: this.userId },
            fetchPolicy: 'no-cache',  // Отключаем кэширование Apollo Client
          });

          console.log('Edited image upload mutation successful, waiting for subscription data...');
          // Теперь ждем данные из подписки
        } catch (graphqlError) {
          this.error = graphqlError;
          console.error('GraphQL error:', graphqlError);
        } finally {
          this.loading = false;
        }
      } catch (error) {
        this.loading = false;
        this.error = error;
        console.error('File upload failed:', error);
      }
    },
  },
};
</script>

<style scoped>
.upload-container {
  border: 2px dashed #ccc;
  padding: 20px;
  text-align: center;
  cursor: pointer;
  margin-top: 20px;
}

.upload-container:hover {
  background-color: #f9f9f9;
}
</style>
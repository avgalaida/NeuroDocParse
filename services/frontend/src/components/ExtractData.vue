<template>
  <div class="upload-container" @drop.prevent="onDrop" @dragover.prevent>
    <input type="file" @change="onFileChange" ref="fileInput" style="display: none" />
    <button @click="triggerFileInput">Загрузить изображение или перетащить сюда</button>
    <div v-if="loading">Загрузка...</div>
    <div v-if="error">Ошибка: {{ error.message }}</div>
    <div v-if="data">
      <DocumentTemplate :document="data.uploadImage" :userId="userId" @update:image="uploadEditedImage" />
    </div>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex';
import { UPLOAD_IMAGE_MUTATION, DATA_EXTRACTED_SUBSCRIPTION } from '@/graphql';
import DocumentTemplate from './DocumentTemplate.vue';
import { v4 as uuidv4 } from 'uuid';

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
      requestId: uuidv4(),
      requestType: 'triple',
      model: 'default',
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
    if (keycloak && keycloak.tokenParsed) {
      this.userId = keycloak.tokenParsed.sub;
    } else {
      console.error('Keycloak token is not available.');
    }

    this.subscribeToDataExtracted();
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

      this.resetState();
      this.loading = true;

      try {
        const reader = new FileReader();
        reader.onload = async () => {
          this.b64Img = reader.result.split(',')[1];

          await this.$apollo.provider.defaultClient.cache.reset();

          try {
            const { data } = await this.$apollo.mutate({
              mutation: UPLOAD_IMAGE_MUTATION,
              variables: {
                b64Img: this.b64Img,
                userId: this.userId,
                requestId: this.requestId,
                requestType: this.requestType,
                model: this.model,
              },
              fetchPolicy: 'no-cache',
            });

            if (data) {
              console.log('Image upload mutation successful, waiting for subscription data...');
            } else {
              throw new Error('No data returned from mutation.');
            }
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
      this.clearImageData();
    },
    async uploadEditedImage(base64Image) {
      this.requestId = uuidv4();
      this.b64Img = base64Image;
      this.loading = true;

      try {
        const { data } = await this.$apollo.mutate({
          mutation: UPLOAD_IMAGE_MUTATION,
          variables: {
            b64Img: this.b64Img,
            userId: this.userId,
            requestId: this.requestId,
            requestType: 'edited',
            model: 'default',
          },
          fetchPolicy: 'no-cache',
        });

        if (data) {
          console.log('Edited image upload mutation successful, waiting for subscription data...');
          this.subscribeToDataExtracted();
        } else {
          throw new Error('No data returned from mutation.');
        }
      } catch (graphqlError) {
        this.error = graphqlError;
        console.error('GraphQL error:', graphqlError);
      } finally {
        this.loading = false;
      }
    },
    subscribeToDataExtracted() {
      this.$apollo.subscribe({
        query: DATA_EXTRACTED_SUBSCRIPTION,
        variables: { requestId: this.requestId },
      }).subscribe({
        next: ({ data }) => {
          this.setImageData(data.dataExtracted);
          this.updateImage(data.dataExtracted.Base64Image);
        },
        error: (err) => console.error('Subscription error:', err),
      });
    },
    updateImage(base64Image) {
      if (this.data && this.data.uploadImage) {
        this.data.uploadImage.Base64Image = base64Image;
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
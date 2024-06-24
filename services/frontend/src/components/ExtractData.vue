<template>
  <div class="upload-container" @drop.prevent="onDrop" @dragover.prevent>
    <input type="file" @change="onFileChange" ref="fileInput" style="display: none" />
    <button @click="triggerFileInput">Загрузить изображение или перетащить сюда</button>
    <div v-if="loading">Загрузка...</div>
    <div v-if="error">Ошибка: {{ error.message }}</div>
    <div v-if="data">Ответ: {{ JSON.stringify(data.uploadImage) }}</div>
  </div>
</template>

<script>
import { gql } from 'graphql-tag';

const UPLOAD_IMAGE_MUTATION = gql`
  mutation UploadImage($b64Img: String!, $userId: String!) {
    uploadImage(b64Img: $b64Img, userId: $userId)
  }
`;

export default {
  name: 'ExtractData',
  data() {
    return {
      loading: false,
      error: null,
      data: null,
      userId: null,
    };
  },
  mounted() {
    const keycloak = this.$keycloak;
    console.log('Keycloak instance:', keycloak);
    if (keycloak && keycloak.tokenParsed) {
      this.userId = keycloak.tokenParsed.sub;
      console.log('User ID:', this.userId);
    } else {
      console.error('Keycloak token is not available.');
    }
  },
  methods: {
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
      this.loading = true;
      this.error = null;
      this.data = null;

      try {
        const reader = new FileReader();
        reader.onload = async () => {
          const b64Img = reader.result.split(',')[1];
          console.log('Image base64:', b64Img);
          console.log('User ID:', this.userId);

          try {
            const { data, errors } = await this.$apollo.mutate({
              mutation: UPLOAD_IMAGE_MUTATION,
              variables: { b64Img, userId: this.userId },
            });

            console.log('GraphQL response:', data);
            console.error('GraphQL errors:', errors);

            if (errors && errors.length) {
              throw new Error(errors[0].message);
            }

            this.loading = false;
            this.data = data;

            if (data && data.uploadImage) {
              this.$emit('document-received', data.uploadImage);
            } else {
              this.error = new Error('Invalid response from server.');
            }
          } catch (graphqlError) {
            this.loading = false;
            this.error = graphqlError;
            console.error('GraphQL error:', graphqlError);
          }
        };
        reader.readAsDataURL(file);
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
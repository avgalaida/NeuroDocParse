<template>
  <div class="document-markup">
    <h2>Разметка документа</h2>
    <form @submit.prevent="submitMarkup">
      <input type="file" @change="onFileChange" ref="fileInput" style="display: none" />
      <button type="button" @click="triggerFileInput">Загрузить изображение</button>
      <div v-if="imageSrc">
        <vue-cropper
          v-if="!isCropped"
          ref="cropper"
          :src="imageSrc"
          :guides="true"
          :view-mode="1"
          :auto-crop-area="0.8"
          class="document-image"
        />
        <div class="cropper-controls" v-if="!isCropped">
          <label for="rotation">Угол поворота:</label>
          <input id="rotation" type="range" min="0" max="360" v-model="rotation" @input="rotateImage" />
          <button type="button" @click="cropAndSaveImage">Обрезать и сохранить</button>
        </div>
        <div v-if="isCropped">
          <canvas ref="annotatedImageCanvas" :width="canvasWidth" :height="canvasHeight" @mousedown="startDrawing" @mousemove="draw" @mouseup="finishDrawing"></canvas>
          <div class="fields-container">
            <div v-for="(field, index) in fields" :key="index" class="field" @click="highlightField(index)">
              <input v-model="field.name" placeholder="Название поля" />
              <button type="button" @click="setFieldMarkup(field)">Выбрать область</button>
            </div>
            <button type="button" @click="addField">Добавить поле</button>
          </div>
          <button type="submit">Отправить на обработку</button>
        </div>
      </div>
      <div v-if="loading">Загрузка...</div>
      <div v-if="error">Ошибка: {{ error.message }}</div>
      <div v-if="result">
        <h2>Результат распознавания</h2>
        <div v-for="(value, key) in result.Fields" :key="key" class="result-field">
          <label :for="key">{{ key }}:</label>
          <input :id="key" v-model="result.Fields[key]" />
        </div>
        <button>Скачать JSON</button>
      </div>
    </form>
  </div>
</template>

<script>
import VueCropper from 'vue-cropperjs';
import 'cropperjs/dist/cropper.css';
import { v4 as uuidv4 } from 'uuid';
import { UPLOAD_IMAGE_WITH_FIELDS_MUTATION } from '@/graphql';
import { mapActions } from 'vuex';

export default {
  name: 'DocumentMarkup',
  components: {
    VueCropper,
  },
  data() {
    return {
      imageSrc: null,
      newCroppedImageSrc: null,
      fields: [],
      loading: false,
      error: null,
      userId: null,
      requestId: uuidv4(),
      rotation: 0,
      activeFieldIndex: null,
      isCropped: false,
      canvasWidth: 0,
      canvasHeight: 0,
      drawing: false,
      startX: 0,
      startY: 0,
      currentWidth: 0,
      currentHeight: 0,
      result: null,
    };
  },
  mounted() {
    const keycloak = this.$keycloak;
    if (keycloak && keycloak.tokenParsed) {
      this.userId = keycloak.tokenParsed.sub;
    } else {
      console.error('Keycloak token is not available.');
    }
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
        this.isCropped = false;
      };
      reader.readAsDataURL(file);
    },
    addField() {
      this.fields.push({ name: '', coordinates: [] });
    },
    setFieldMarkup(field) {
      if (this.$refs.annotatedImageCanvas) {
        const rect = this.$refs.annotatedImageCanvas.getBoundingClientRect();
        field.coordinates = [
          this.startX,
          this.startY,
          this.startX + this.currentWidth,
          this.startY + this.currentHeight,
        ];
      }
    },
    selectFieldArea(event) {
      if (this.activeFieldIndex !== null) {
        const field = this.fields[this.activeFieldIndex];
        const rect = this.$refs.annotatedImageCanvas.getBoundingClientRect();
        field.coordinates = [
          event.clientX - rect.left,
          event.clientY - rect.top,
          event.clientX - rect.left + 100,
          event.clientY - rect.top + 50,
        ];
        this.drawBoundingBox(field.coordinates[0], field.coordinates[1], 100, 50);
      }
    },
    highlightField(index) {
      this.activeFieldIndex = index;
      const field = this.fields[index];
      if (field.coordinates.length) {
        const [x, y, x2, y2] = field.coordinates;
        const width = x2 - x;
        const height = y2 - y;
        this.drawBoundingBox(x, y, width, height);
      }
    },
    drawBoundingBox(x, y, width, height) {
      const canvas = this.$refs.annotatedImageCanvas;
      const ctx = canvas.getContext('2d');
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      const img = new Image();
      img.src = this.newCroppedImageSrc;
      img.onload = () => {
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        ctx.beginPath();
        ctx.rect(x, y, width, height);
        ctx.lineWidth = 2;
        ctx.strokeStyle = 'red';
        ctx.stroke();
      };
    },
    rotateImage() {
      if (this.$refs.cropper) {
        this.$refs.cropper.cropper.rotateTo(this.rotation);
      }
    },
    cropAndSaveImage() {
      if (this.$refs.cropper) {
        const cropper = this.$refs.cropper.cropper;
        const croppedCanvas = cropper.getCroppedCanvas();
        croppedCanvas.toBlob((blob) => {
          const reader = new FileReader();
          reader.onloadend = () => {
            this.newCroppedImageSrc = reader.result;
            this.canvasWidth = croppedCanvas.width;
            this.canvasHeight = croppedCanvas.height;
            this.isCropped = true;
            this.$nextTick(() => {
              this.drawBoundingBox(0, 0, 0, 0); // Draw the initial image on canvas
            });
          };
          reader.readAsDataURL(blob);
        });
      }
    },
    startDrawing(event) {
      const rect = this.$refs.annotatedImageCanvas.getBoundingClientRect();
      this.startX = event.clientX - rect.left;
      this.startY = event.clientY - rect.top;
      this.drawing = true;
    },
    draw(event) {
      if (!this.drawing) return;
      const rect = this.$refs.annotatedImageCanvas.getBoundingClientRect();
      const ctx = this.$refs.annotatedImageCanvas.getContext('2d');
      const currentX = event.clientX - rect.left;
      const currentY = event.clientY - rect.top;
      this.currentWidth = currentX - this.startX;
      this.currentHeight = currentY - this.startY;
      ctx.clearRect(0, 0, this.$refs.annotatedImageCanvas.width, this.$refs.annotatedImageCanvas.height);
      const img = new Image();
      img.src = this.newCroppedImageSrc;
      img.onload = () => {
        ctx.drawImage(img, 0, 0, this.$refs.annotatedImageCanvas.width, this.$refs.annotatedImageCanvas.height);
        ctx.beginPath();
        ctx.rect(this.startX, this.startY, this.currentWidth, this.currentHeight);
        ctx.lineWidth = 2;
        ctx.strokeStyle = 'red';
        ctx.stroke();
      };
    },
    finishDrawing() {
      this.drawing = false;
    },
    async submitMarkup() {
      if (!this.newCroppedImageSrc || !this.fields.length || !this.userId) return;

      this.loading = true;
      this.error = null;

      try {
        const fieldsData = {};

        this.fields.forEach((field) => {
          if (field.name && field.coordinates.length) {
            fieldsData[field.name] = [field.coordinates];
          }
        });

        const { data } = await this.$apollo.mutate({
          mutation: UPLOAD_IMAGE_WITH_FIELDS_MUTATION,
          variables: {
            b64Img: this.newCroppedImageSrc.split(',')[1],
            userId: this.userId,
            requestId: this.requestId,
            model: 'default',
            fields: fieldsData,
          },
        });

        if (data) {
          this.result = data.uploadImageWithFields;
          console.log('Image upload with fields mutation successful.');
        } else {
          throw new Error('No data returned from mutation.');
        }
      } catch (error) {
        this.error = error;
        console.error('GraphQL error:', error);
      } finally {
        this.loading = false;
      }
    },
  },
};
</script>

<style scoped>
.document-markup {
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

.fields-container {
  margin-top: 20px;
}

.field {
  margin-bottom: 10px;
}

.cropper-controls {
  margin-top: 10px;
}

input[type="text"] {
  width: 100%;
  padding: 5px;
  margin-bottom: 5px;
}

.document-image {
  max-width: 100%;
  height: auto;
}

.results-container {
  margin-top: 20px;
}

.result-field {
  margin-bottom: 10px;
}

.result-field input {
  width: 100%;
  padding: 5px;
}
</style>
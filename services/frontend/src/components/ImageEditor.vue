<template>
  <div class="image-editor">
    <vue-cropper
      ref="cropper"
      :src="imageSrc"
      :guides="true"
      :view-mode="1"
      :auto-crop-area="1.0"
      class="document-image"
    />
    <div class="cropper-controls">
      <label for="rotation">Угол поворота:</label>
      <input id="rotation" type="range" min="0" max="360" v-model="rotation" @input="rotateImage" />
      <button @click="cropImage">Обрезать</button>
    </div>
  </div>
</template>

<script>
import VueCropper from 'vue-cropperjs';
import 'cropperjs/dist/cropper.css';

export default {
  name: 'ImageEditor',
  components: {
    VueCropper,
  },
  props: {
    imageSrc: {
      type: String,
      required: true,
    },
    userId: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      cropper: null,
      rotation: 0,
    };
  },
  methods: {
    onCropperReady() {
      this.cropper = this.$refs.cropper.cropper;
      this.cropper.crop(); // Убедимся, что область обрезки установлена
    },
    rotateImage() {
      if (this.cropper) {
        this.cropper.rotateTo(this.rotation);
      }
    },
    cropImage() {
      if (this.cropper) {
        const croppedCanvas = this.cropper.getCroppedCanvas();
        croppedCanvas.toBlob((blob) => {
          const reader = new FileReader();
          reader.onloadend = () => {
            const base64data = reader.result.split(',')[1];
            this.$emit('update:image', base64data);
          };
          reader.readAsDataURL(blob);
        });
      }
    },
  },
  mounted() {
    this.onCropperReady();
  },
};
</script>

<style scoped>
.image-editor {
  text-align: center;
}

.document-image {
  max-width: 100%;
  height: auto;
}

.cropper-controls {
  margin-top: 10px;
}

button {
  margin: 5px;
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
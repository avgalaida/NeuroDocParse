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
      </div>
    </div>
  </template>
  
  <script>
  import VueCropper from 'vue-cropperjs';
  import 'cropperjs/dist/cropper.css';
  
  export default {
    name: 'ImageEditorSelect',
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
        this.cropper.crop();
      },
      rotateImage() {
        if (this.cropper) {
          this.cropper.rotateTo(this.rotation);
        }
      },
      getCroppedImage() {
        if (this.cropper) {
          const croppedCanvas = this.cropper.getCroppedCanvas();
          return new Promise((resolve) => {
            croppedCanvas.toBlob((blob) => {
              const reader = new FileReader();
              reader.onloadend = () => {
                const base64data = reader.result.split(',')[1];
                resolve(base64data);
              };
              reader.readAsDataURL(blob);
            });
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
  
  label {
    margin-right: 10px;
  }
  
  input[type="range"] {
    vertical-align: middle;
  }
  </style>
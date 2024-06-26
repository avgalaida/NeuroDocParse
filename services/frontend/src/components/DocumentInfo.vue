<template>
  <div class="document-info" v-if="document">
    <h3>Информация о документе</h3>
    <p><strong>ClientId:</strong> {{ document.ClientId || 'N/A' }}</p>
    <p><strong>BucketName:</strong> {{ document.BucketName || 'N/A' }}</p>
    <p><strong>ObjectName:</strong> {{ document.ObjectName || 'N/A' }}</p>
    <p><strong>DocumentName:</strong> {{ document.DocumentName || 'N/A' }}</p>
  </div>
  <div v-else>
    <p>Нет данных для отображения.</p>
  </div>
</template>

<script>
export default {
  name: 'DocumentInfo',
  props: ['document'],
};
</script>

<style scoped>
.document-info {
  border: 1px solid #ccc;
  padding: 15px;
  margin-bottom: 20px;
  background-color: #f9f9f9;
}
</style>
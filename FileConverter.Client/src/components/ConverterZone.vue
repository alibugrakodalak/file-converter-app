<template>
  <div class="glass-card">
    <div class="card-header">
      <div class="icon-bg">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="32"
          height="32"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
          class="header-icon"
        >
          <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"></path>
          <polyline points="14 2 14 8 20 8"></polyline>
        </svg>
      </div>
      <h1>Dosya Dönüştürücü</h1>
      <p>Excel, PNG veya TXT dosyalarınızı saniyeler içinde dönüştürün.</p>
    </div>

    <div
      class="drop-zone"
      :class="{ 'active-drop': isDragging }"
      @dragover.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @drop.prevent="handleDrop"
      @click="triggerFileInput"
    >
      <input
        type="file"
        ref="fileInput"
        @change="handleFileChange"
        accept=".xlsx, .png, .txt"
        hidden
      />

      <div v-if="!selectedFile" class="drop-content">
        <div class="cloud-icon">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="48"
            height="48"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.5"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
            <polyline points="17 8 12 3 7 8"></polyline>
            <line x1="12" y1="3" x2="12" y2="15"></line>
          </svg>
        </div>
        <p class="drop-text">Dosyayı buraya sürükleyin veya <span>seçmek için tıklayın</span></p>
        <p class="support-text">Desteklenenler: XLSX, PNG, TXT</p>
      </div>

      <div v-else class="file-selected">
        <div class="file-info">
          <span class="file-icon">📄</span>
          <div class="file-details">
            <span class="file-name">{{ selectedFile.name }}</span>
            <span class="file-size">{{ (selectedFile.size / 1024).toFixed(1) }} KB</span>
          </div>
        </div>
        <button @click.stop="removeFile" class="remove-btn">✕</button>
      </div>
    </div>

    <transition name="fade">
      <div v-if="errorMessage" class="error-badge">⚠️ {{ errorMessage }}</div>
    </transition>

    <button
      @click="uploadFile"
      :disabled="!selectedFile || isLoading"
      class="action-btn"
      :class="{ loading: isLoading }"
    >
      <span v-if="!isLoading">{{ buttonText }}</span>
      <span v-else class="loader"></span>
    </button>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import api from "../services/api";

// --- State ---
const selectedFile = ref(null);
const isLoading = ref(false);
const errorMessage = ref("");
const isDragging = ref(false); // Sürükleme efekti için
const fileInput = ref(null); // Input elementine erişmek için

// --- Computed ---
const buttonText = computed(() => {
  if (!selectedFile.value) return "Lütfen Dosya Seçin";

  const ext = selectedFile.value.name.split(".").pop().toLowerCase();
  if (ext === "xlsx") return "PDF'e Dönüştür";
  if (ext === "png") return "JPG'e Dönüştür";
  if (ext === "txt") return "ZIP Dosyası Yap";
  return "Dönüştür";
});

// --- Actions ---
const triggerFileInput = () => {
  fileInput.value.click();
};

const handleFileChange = (event) => {
  const file = event.target.files[0];
  validateAndSetFile(file);
};

const handleDrop = (event) => {
  isDragging.value = false;
  const file = event.dataTransfer.files[0];
  validateAndSetFile(file);
};

const validateAndSetFile = (file) => {
  if (!file) return;

  // Basit uzantı kontrolü
  const validExtensions = ["xlsx", "png", "txt"];
  const ext = file.name.split(".").pop().toLowerCase();

  if (!validExtensions.includes(ext)) {
    errorMessage.value = "Desteklenmeyen dosya türü!";
    selectedFile.value = null;
    return;
  }

  selectedFile.value = file;
  errorMessage.value = "";
};

const removeFile = () => {
  selectedFile.value = null;
  errorMessage.value = "";
  // Input değerini sıfırla ki aynı dosyayı tekrar seçebilsin
  if (fileInput.value) fileInput.value.value = "";
};

const getTargetExtension = (fileName) => {
  const ext = fileName.split(".").pop().toLowerCase();
  if (ext === "xlsx") return "pdf";
  if (ext === "png") return "jpg";
  if (ext === "txt") return "zip";
  return "converted";
};

const uploadFile = async () => {
  if (!selectedFile.value) return;

  isLoading.value = true;
  errorMessage.value = "";

  try {
    const formData = new FormData();
    formData.append("file", selectedFile.value);

    const response = await api.post("/converter/upload", formData, {
      responseType: "blob",
    });

    downloadFile(response, selectedFile.value.name);
  } catch (error) {
    console.error(error);
    errorMessage.value = "Dönüştürme başarısız oldu. Sunucu hatası.";
  } finally {
    isLoading.value = false;
  }
};

const downloadFile = (response, originalFileName) => {
  const url = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement("a");
  link.href = url;

  const targetExt = getTargetExtension(originalFileName);
  const newFileName = originalFileName.split(".")[0] + "." + targetExt;

  link.setAttribute("download", newFileName);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
};
</script>

<style scoped>
/* Glassmorphism Card */
.glass-card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 40px;
  border-radius: 24px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
  width: 100%;
  max-width: 480px;
  text-align: center;
  transition: transform 0.3s ease;
}

.glass-card:hover {
  transform: translateY(-5px);
}

/* Header */
.card-header {
  margin-bottom: 30px;
}
.icon-bg {
  width: 60px;
  height: 60px;
  background: #eef2ff;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 15px;
  color: #6366f1;
}
h1 {
  margin: 0;
  font-size: 24px;
  color: #1e293b;
  font-weight: 700;
}
p {
  margin: 8px 0 0;
  color: #64748b;
  font-size: 14px;
}

/* Drop Zone */
.drop-zone {
  border: 2px dashed #cbd5e1;
  border-radius: 16px;
  padding: 30px 20px;
  cursor: pointer;
  transition: all 0.3s ease;
  background: #f8fafc;
  min-height: 150px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.drop-zone:hover,
.drop-zone.active-drop {
  border-color: #6366f1;
  background: #eef2ff;
}

.cloud-icon {
  color: #94a3b8;
  margin-bottom: 10px;
}
.drop-text {
  font-size: 15px;
  color: #334155;
  font-weight: 500;
}
.drop-text span {
  color: #6366f1;
  text-decoration: underline;
}
.support-text {
  font-size: 12px;
  color: #94a3b8;
  margin-top: 5px;
}

/* Seçili Dosya Görünümü */
.file-selected {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 10px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}
.file-info {
  display: flex;
  align-items: center;
  gap: 12px;
  text-align: left;
}
.file-icon {
  font-size: 24px;
}
.file-name {
  display: block;
  font-weight: 600;
  font-size: 14px;
  color: #334155;
}
.file-size {
  display: block;
  font-size: 11px;
  color: #94a3b8;
}
.remove-btn {
  background: none;
  border: none;
  color: #ef4444;
  font-size: 18px;
  cursor: pointer;
  padding: 5px;
}

/* Buton */
.action-btn {
  margin-top: 25px;
  width: 100%;
  padding: 16px;
  border: none;
  border-radius: 12px;
  background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
  color: white;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.4);
}

.action-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(99, 102, 241, 0.5);
}

.action-btn:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
  box-shadow: none;
}

/* Hata Mesajı */
.error-badge {
  margin-top: 15px;
  background: #fef2f2;
  color: #ef4444;
  padding: 10px;
  border-radius: 8px;
  font-size: 13px;
  border: 1px solid #fee2e2;
}

/* Yükleniyor Animasyonu */
.loader {
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: #fff;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* Animasyonlar */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>

import axios from "axios";

// Backend adresimiz (Visual Studio'da gördüğün portu buraya yazman gerekecek)
// Portu 5000'den 5050'ye çekiyoruz
const apiClient = axios.create({
  baseURL: "http://localhost:5050/api", // <-- DİKKAT: 5050 OLDU
  headers: {
    "Content-Type": "multipart/form-data",
  },
});

export default apiClient;

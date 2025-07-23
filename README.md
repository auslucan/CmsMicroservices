# Proje Amacı
Bu projede, kurumların dijital içeriklerini (örneğin haberler, duyurular, belgeler) merkezi bir sistem üzerinden yönetebilmesi için mikroservis mimarisine dayalı bir yapı geliştirildi. İki ana servis yer alıyor: ContentService (içerik yönetimi) ve UserService (kullanıcı yönetimi). Amaç, servislerin birbiriyle entegre şekilde çalışabildiği, izlenebilir, dayanıklı ve yönetilebilir bir sistem sunmaktı.

---

## 🔧 Servisler ve Görevleri

### 🧑‍💼 UserService
- Kullanıcı kayıt, giriş ve doğrulama işlemlerini gerçekleştirir.
- PostgreSQL veri tabanı kullanır.
- Servis keşfi için Consul entegredir.
- İzlenebilirlik için Jaeger ile desteklenmiştir.
- API'lere Swagger arayüzü üzerinden erişebilirsiniz.

### 📰 ContentService
- İçerik oluşturma, güncelleme ve silme işlemlerini yönetir.
- Kullanıcı doğrulaması için UserService’e HTTP istekleri gönderir.
- Polly ile retry ve circuit breaker desteği içerir.
- PostgreSQL, Consul ve Jaeger entegrasyonları mevcuttur.
- Swagger UI üzerinden testler yapabilirsiniz.

---

## 🔍 Geliştirici Araçları ve İzleme

### 📍 Swagger UI Erişimi  
Her servis için Swagger arayüzüne aşağıdaki adreslerden erişebilirsiniz (örnek portlar verilmiştir):

- http://localhost:5001/swagger → UserService  
- http://localhost:5002/swagger → ContentService  

API çağrıları yaparken `x-api-key` header'ı olarak şu anahtarı kullanabilirsiniz:  
`x-api-key: supersecretkey`  

Swagger arayüzünde de sağ üstteki **Authorize** butonuna bu API key’i girerek yetkili şekilde API'leri test edebilirsiniz.

---

### 🛰️ Jaeger Tracing Paneli  
Servisler arası tüm çağrıları uçtan uca izlemek için aşağıdaki adresten Jaeger arayüzüne erişebilirsiniz:  
http://localhost:16686  

Buradan ContentService’in UserService’e yaptığı çağrılar dahil olmak üzere tüm işlemlerin süresini ve izleme detaylarını görebilirsiniz.

---

### 🔐 Yapılandırma ve Consul  
Connectionstring ve api-key appsettingse eklenmiştir ancak ek geliştirme olarak gelecekte, tüm yapılandırma verileri (örneğin API key, connection string gibi) Consul KV store’dan okunabilir. Böylece servisler yapılandırmalarını merkezi bir kaynaktan güvenli ve dinamik olarak alabilirler.

---

## 🧪 Testler  
Projede birim testler xUnit ve Moq kütüphaneleri kullanılarak yazıldı.  
- Servislerin iş mantığı test edildi.  
- API endpointleri mocklarla izole edildi.  


Böylece kod kalitesi yükseltildi, hata riskleri azaltıldı.

---

## 🐳 Docker Compose ile Hızlı Kurulum  
Tüm sistem bileşenlerini (UserService, ContentService, PostgreSQL, Jaeger, Consul) aynı anda ayağa kaldırmak için aşağıdaki komutu çalıştırabilirsiniz:
`docker-compose up --build`

Bu sayede:

PostgreSQL veri tabanları otomatik kurulur,
Servisler Consul üzerinden birbirini tanır,
Jaeger trace toplamaya başlar.

🧠 Kullanılan Teknolojiler

.NET 8
PostgreSQL
Docker & Docker Compose
Consul (servis keşfi)
Jaeger + OpenTelemetry (izleme)
Polly (retry & circuit breaker)
Swagger (API dokümantasyonu)
xUnit & Moq (birim testler)





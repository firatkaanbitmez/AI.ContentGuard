# AI.ContentGuard

## Proje Amacı

HTML, PlainText, JSON ve görsel içerikleri analiz ederek;

* Spam riski tespiti
* Phishing / Dolandırıcılık içerik analizi
* SQL Injection / Script kontrolü
* Görsel manipülasyon ve NSFW tespiti
* Yasaklı kelime, domain, görsel tespiti
* İçerik düzeltme önerisi üretimi

---

## Vizyon

Proaktif, ölçeklenebilir ve AI destekli bir **içerik güvenlik duvarı (Content Security Barrier)** oluşturmak.

---

## Mimarinin Temel Yaklaşımı

* **Clean Architecture**
* **Pipeline Bazlı İşleyiş**
* **Event-Driven Mimari**
* **AI Hybrid Model (Kural + AI Karışımı)**

---

## Ana Hedefler

* **Maliyet / Performans Optimizasyonu**
* **Katmanlı AI kontrolü (LLM minimum maliyetle kullanılır)**
* **Esnek genişletilebilirlik (Plug & Play modüller)**
* **Tam izlenebilirlik ve Audit loglama**

---

## Klasör Yapısı

```
├── AI.ContentGuard.Domain/         # Entity, ValueObject, İş Kuralları
├── AI.ContentGuard.Application/    # UseCase, DTO, Handler, Service
├── AI.ContentGuard.Infrastructure/ # DB, AI, OCR, CNN, LLM, API Adapter
├── AI.ContentGuard.API/            # REST API (ASP.NET Core Web API)
├── AI.ContentGuard.Worker/         # RabbitMQ Consumer & Pipeline Orkestrasyon
├── AI.ContentGuard.Shared/         # Ortak Exception, Result, Logging, Utils
```

---

## Kullanılan Teknolojiler

| Alan       | Teknoloji               |
| ---------- | ----------------------- |
| Backend    | .NET 8, C#              |
| Database   | PostgreSQL, EF Core 8   |
| Messaging  | RabbitMQ                |
| AI (Text)  | Azure OpenAI, LLM       |
| AI (Image) | Tesseract OCR, CNN      |
| Caching    | Redis                   |
| Logging    | Serilog + Elastic APM   |
| Deployment | Azure Pipelines, Docker |

---

## Ana Modüller ve Sorumluluklar

| Modül                   | Görev                                   |
| ----------------------- | --------------------------------------- |
| TemplateAnalysisService | HTML, JSON, PlainText parse & normalize |
| SpamDetectionEngine     | LLM + Rule Engine spam tespiti          |
| ImageAnalysisPipeline   | Katmanlı görsel analiz                  |
| InjectionValidator      | SQL/XSS kontrolü                        |
| ScoreCalculator         | Risk skoru hesaplama                    |
| RecommendationEngine    | Güvenli içerik önerisi üretimi          |
| AuditLogger             | İzlenebilirlik, işlem kaydı             |
| FeedbackHandler         | AI modeli iyileştirme (Feedback Loop)   |

---

## Gelişmiş Katmanlı Görsel Analiz (Maliyet Optimizasyonu)

| Katman   | İşlem            | Amaç                                 | Maliyet                |
| -------- | ---------------- | ------------------------------------ | ---------------------- |
| Katman 0 | Metadata Check   | Boyut, format kontrolü               | Çok Düşük              |
| Katman 1 | OCR + Hash Check | Yazı kontrol, görsel hash            | Düşük                  |
| Katman 2 | CNN Model        | Spam/NSFW Tespiti                    | Orta                   |
| Katman 3 | LLM Captioning   | Anlam analizi, manipülasyon kontrolü | Yüksek (Az tetiklenir) |

### Yazı Olmayan Görsellerde Maliyet Kontrolü

* **Text Presence Detector (TPD)** kullanılacak.
* OCR sonucu boşsa TPD devreye girer.
* Görselde yazı yoksa doğrudan Katman 2’ye geçilir.
* **Tesseract gereksiz çalıştırılmaz**, maliyet azalır.
* `.phash` ve `.ahash` ile hızlı blacklist/whitelist kontrolü yapılır.
* CNN ile NSFW + manipülasyon tespiti yapılır.
* LLM sadece belirsiz durumlarda tetiklenir.

---

## Veritabanı Tasarımı (PostgreSQL)

| Tablo              | Açıklama                 |
| ------------------ | ------------------------ |
| analysis\_requests | Gelen istek kayıtları    |
| analysis\_results  | İşlem sonuçları          |
| detected\_issues   | Bulunan problemler       |
| spam\_rules        | Dinamik kural seti       |
| customer\_profiles | Müşteri bazlı risk ayarı |
| image\_hashes      | Görsel hash veri seti    |
| audit\_logs        | Detaylı işlem izleri     |

---

## Spam Skorlama

| Risk Türü        | Puan |
| ---------------- | ---- |
| SQL Injection    | +30  |
| Yasaklı Link     | +20  |
| LLM Spam Analizi | +40  |
| Görsel Spam/NSFW | +50  |
| Blacklist Kelime | +15  |

---

## Risk Seviyeleri

| Skor    | Risk              |
| ------- | ----------------- |
| 0-40    | Güvenli           |
| 41-60   | Düşük Risk        |
| 61-80   | Orta Risk         |
| 81-100+ | Yüksek Risk - Ret |

---

## İşleyiş Akışı

1. API üzerinden içerik alınır.
2. RabbitMQ ile iş kuyruğuna atılır.
3. Pipeline Worker devreye girer:

   * Metin Analizi (Rule + LLM)
   * Görsel Analizi (Katmanlı Yapı)
4. Skorlama yapılır, veritabanına kaydedilir.
5. Sonuç API üzerinden sorgulanabilir.

---

## Hibrit AI + Rule Modeli

| Alan     | Yöntem                           |
| -------- | -------------------------------- |
| Metin    | Regex + Blacklist + LLM          |
| Görsel   | OCR + CNN + LLM Caption          |
| JSON     | Parametre kontrolü, schema check |
| Feedback | Aktif öğrenme, retraining        |

---

## Gelecek Genişletme Planı

* Kendi LLM modelini eğitme (Maliyet daha da azaltılır)
* Çoklu dil spam & phishing analizi
* JSON parametre & context aware kontrol
* Anlık Dashboard (Grafana / Kibana)
* WebSocket ile anlık bildirim
* Multi-tenant mimari desteği

---

## Ek İyileştirmeler (Profesyonel Yaklaşım)

* **CI/CD Pipeline:** Azure Pipelines + SonarQube + Security Scan
* **Test Coverage:** xUnit + Moq + AutoFixture + TestContainers (PostgreSQL)
* **Observability:** Elastic APM + Serilog + OpenTelemetry
* **Containerization:** Dockerfile + Kubernetes Ops hazırlığı (Future-proof)

---

AI.ContentGuard Project
Amaç
HTML, PlainText, JSON ve görsel içerikleri analiz ederek;
Spam riski tespiti


Phishing / Dolandırıcılık içerik analizi


SQL Injection / Script kontrolü


Görsel manipülasyon ve NSFW tespiti


Yasaklı kelime, domain, görsel tespiti


İçerik düzeltme önerisi


Vizyon
Proaktif, ölçeklenebilir, AI destekli bir içerik güvenlik duvarı (Content Security Barrier) oluşturmak.
Mimarinin Temel Yaklaşımı
Clean Architecture + Pipeline + Event-Driven + AI Hybrid Model
Ana Hedefler:
Maliyet / Performans optimizasyonu


Katmanlı AI kontrolü (LLM minimum maliyetle kullanılır)


Esnek genişletilebilirlik (Plug & Play modüller)


Tam izlenebilirlik ve Audit loglama



Klasör Yapısı

├── AI.ContentGuard.Domain/         # Entity, ValueObject, İş Kuralları
├── AI.ContentGuard.Application/    # UseCase, DTO, Handler, Service
├── AI.ContentGuard.Infrastructure/ # DB, AI, OCR, CNN, LLM, API Adapter
├── AI.ContentGuard.API/            # REST API (ASP.NET Core Web API)
├── AI.ContentGuard.Worker/         # RabbitMQ Consumer & Pipeline Orkestrasyon
├── AI.ContentGuard.Shared/         # Ortak Exception, Result, Logging, Utils

Teknolojiler
Alan
Teknoloji
Backend
.NET 8, C#
Database
PostgreSQL, EF Core 8
Messaging
RabbitMQ
AI (Text)
Azure OpenAI, LLM
AI (Image)
Tesseract OCR, CNN
Caching
Redis
Logging
Serilog + Elastic APM
Deployment
Azure Pipelines, Docker


Ana Modüller ve Sorumluluklar
Modül
Görev
TemplateAnalysisService
HTML, JSON, PlainText parse ve normalize
SpamDetectionEngine
LLM + Rule Engine spam tespiti
ImageAnalysisPipeline
Katmanlı görsel analiz
InjectionValidator
SQL/XSS kontrolü
ScoreCalculator
Risk skoru hesaplama
RecommendationEngine
Güvenli içerik önerisi üretimi
AuditLogger
İzlenebilirlik, işlem kaydı
FeedbackHandler
Sonuçlara göre AI modeli iyileştirme


Gelişmiş Katmanlı Görsel Analiz (Maliyet Optimize)
Katman
İşlem
Amaç
Maliyet
Katman 0
Metadata Check
Boyut, format kontrolü
Çok Düşük
Katman 1
OCR + Hash Check
Yazı kontrol, görsel hash karşılaştırma
Düşük
Katman 2
CNN Model
Spam/NSFW Tespiti
Orta
Katman 3
LLM Captioning
Anlam analizi, manipülasyon kontrol
Yüksek (Az tetiklenir)

Yazı Olmayan Görsellerde Maliyet Kontrol Çözümü
Katman 1'de Text Presence Detector (TPD) kullanılacak.


OCR sonuçları boşsa, TPD devreye girer.


Eğer görselde yazı yoksa doğrudan Katman 2’ye geçilir.


Tesseract gereksiz yere çalıştırılmaz, bu da maliyeti azaltır.


Ek Avantaj:
Görseller için ".phash" & ".ahash" ile hızlı blacklist/whitelist taraması.


CNN modeli ile NSFW + manipülasyon tespiti


LLM sadece ihtiyaca göre tetiklenir (örn: belirsiz durumlarda)



Veritabanı Tasarımı (PostgreSQL)
Tablo
Açıklama
analysis_requests
Gelen istek kayıtları
analysis_results
İşlem sonuçları
detected_issues
Bulunan problemler
spam_rules
Dinamik kural seti
customer_profiles
Müşteri bazlı risk ayarı
image_hashes
Görsel hash veri seti
audit_logs
Detaylı işlem izleri


Spam Skorlama
Risk Türü
Puan
SQL Injection
+30
Yasaklı Link
+20
LLM Spam Analizi
+40
Görsel Spam/NSFW
+50
Blacklist Kelime
+15

Risk Seviyeleri
Skor
Risk
0-40
Güvenli
41-60
Düşük Risk
61-80
Orta Risk
81-100+
Yüksek Risk - Ret


Akış
API üzerinden içerik alınır


RabbitMQ ile iş kuyruğuna atılır


Pipeline Worker devreye girer:


Metin Analizi (Rule + LLM)


Görsel Analizi (Katmanlı Yapı)


Skorlama yapılır, veritabanına kayıt edilir


Sonuç API üzerinden sorgulanabilir



Hibrit AI + Rule Modeli
Alan
Yöntem
Metin
Regex + Blacklist + LLM
Görsel
OCR + CNN + LLM Caption
JSON
Parametre kontrolü, schema check
Feedback
Aktif öğrenme, retraining, model güncelleme


Gelecek Genişletme Planı
Kendi LLM modelini eğitme (Maliyet daha da azaltılır)


Çoklu dil spam & phishing analizi


JSON parametre & context aware kontrol


Anlık Dashboard (Grafana / Kibana ile)


WebSocket ile anlık bildirim entegrasyonu


Multi-tenant mimari desteği



Ek İyileştirmeler (Profesyonel Yaklaşım)
CI/CD Pipeline: Azure Pipelines + SonarQube + Security Scan


Test Coverage: xUnit + Moq + AutoFixture + TestContainers (PostgreSQL)


Observability: Elastic APM + Serilog + OpenTelemetry


Containerization: Dockerfile + Kubernetes Ops hazırlığı (Future-proof)





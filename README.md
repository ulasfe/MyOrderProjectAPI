Proje sipariş oluşturma ve sipariş takibini amaçlayan bir projedir.
Uygulama CodeFirst yaklaşımı ile çalışmaktadır.

Swagger kullanımı ile API request testlerini gerçekleştirebilir, gerekli olan json format bilgilerini edinebilirsiniz.
Gerekli görüldüğü durumlarda Swagger port yapılandırmaları için MyOrderPojectAPI/Properties altındaki launchSettings.json dosyasını dzenlemeniz gerekmektedir.
Proje kendi içinde ufak birim testleri ile derleme öncesi temel fonksiyon kontrolünün yapılmasını da sağlayabilir.

DB oluşturma ve bağlantı için 

1.) appsettings.json dosyası içindeki DefaultConnection satırında düzenleme yapılması gereklidir

2.) DB oluşturmak için gerekli bash kodları sırasıyla :

        A) dotnet ef migrations add InitialMigration --project MyOrderProjectAPI
        B) dotnet ef database update --project MyOrderProjectAPI

Akış Diyagramı:
1.	İstemci (Web/Mobil Uyg.)	İstek Gönderir	Kullanıcı, sipariş içeriğini onayladıktan sonra API'ye bir POST isteği (/api/orders) gönderir.
2.	MyOrderProjectAPI (Controller)	İsteği Alır	Endpoint, gelen isteği yakalar ve yetkilendirme (Authentication/Authorization) kontrolünü başlatır.
3.	MyOrderProjectAPI (Service Katmanı)	İş Mantığını Uygular	Sipariş detaylarını doğrular, stok kontrolü yapar, fiyatı hesaplar ve gerekli iş kurallarını işletir.
4.	MyOrderProjectAPI (Repository Katmanı)	Veri Kaydı Yapar	İşlenmiş ve doğrulanmış sipariş verilerini Veritabanı'na kaydetmek için ilgili komutu iletir.
5.	Veritabanı (DB)	Veriyi Kalıcı Hale Getirir	Siparişi kaydeder ve başarılı işlem sonucunu API'ye geri gönderir.
6.	MyOrderProjectAPI (Controller)	Yanıt Oluşturur	İstemciye başarılı bir durum kodu (HTTP 201 Created veya 200 OK) ve oluşturulan siparişin detaylarını içeren bir JSON yanıtı döndürür.

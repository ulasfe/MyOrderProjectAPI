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

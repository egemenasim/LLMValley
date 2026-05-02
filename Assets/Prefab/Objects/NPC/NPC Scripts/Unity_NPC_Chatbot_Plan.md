# Unity NPC AI Chatbot Sistem Planı (OpenRouter)

Bu doküman, Unity üzerinde OpenRouter API kullanarak NPC'lerle (Non-Player Characters) etkileşime girebileceğiniz dinamik bir chatbot sisteminin mimarisini ve geliştirme adımlarını detaylandırır. Sistem `egemenclaw` projesindeki gibi sadece ücretsiz (free) modelleri çekme ve kullanma özelliğini barındıracaktır.

## 1. Mimari Genel Bakış

Bu sistem, 4 ana bileşene ayrılmaktadır:
1. **Veri Modeli (ScriptableObject):** NPC'nin kimliğini ve özelliklerini barındırır.
2. **API Yöneticisi:** OpenRouter'a bağlanır, ücretsiz modelleri çeker ve mesajlaşmayı yönetir.
3. **Etkileşim Yöneticisi:** Oyuncunun NPC yanına gelip "E" tuşuna basmasını dinler.
4. **UI Yöneticisi:** Unity Canvas üzerinden mesajları gösterir ve oyuncunun cevap yazmasını sağlar.

---

## 2. Geliştirme Adımları ve Kod Yapıları

### A. NPC Persona Tanımlaması (ScriptableObject)
NPC'lerin birbirlerinden farklı karakterlere sahip olmasını sağlamak için `ScriptableObject` kullanacağız. Bu sayede Unity Inspector üzerinden kod yazmadan yeni NPC çeşitleri üretebileceğiz.

**`NPCPersona.cs`**
- `npcName` (string): NPC'nin ekranda gözükecek ismi.
- `systemPrompt` (string - TextArea): NPC'nin impersonate edeceği rol. Örn: *"Sen huysuz bir demircisin. Müşterilere ters cevap ver."*

### B. OpenRouter API İletişimi (API Yöneticisi)
API iletişiminde 2 temel görevimiz var: Ücretsiz modelleri çekmek ve sohbet mesajlarını alıp/göndermek.

**`OpenRouterClient.cs` (MonoBehaviour)**
- **Inspector Alanları:**
  - `apiKey` (string): OpenRouter API anahtarı.
  - `selectedModel` (string): Kullanılacak modelin ID'si. Açılır menü listesiyle (Dropdown) dinamik olarak inspector veya UI'dan seçilebilir.
- **Fonksiyonlar:**
  - `FetchFreeModels()`: `https://openrouter.ai/api/v1/models` adresine istek atar. Gelen JSON'u parse eder, `pricing.prompt = "0"` ve `pricing.completion = "0"` olanları filtreleyip kullanılabilir modeller dizisine aktarır.
  - `SendChatCompletion(string prompt, List<Message> history)`: Geçmiş mesajları ve system prompt'u birleştirerek `https://openrouter.ai/api/v1/chat/completions` ucuyla (endpoint) iletişim kurar.

*(Not: Unity `JsonUtility` karmaşık JSON verilerini parse ederken zorlanabildiğinden, `Newtonsoft.Json` kütüphanesini kullanmak bu aşamada projeye dahil edilmelidir.)*

### C. NPC Etkileşim ve Trigger Sistemi (NPC Controller)
Oyuncunun dünyaya yerleştirilen NPC ile fiziksel bağ kurmasını sağlar.

**`NPCInteraction.cs` (MonoBehaviour)**
- **Bileşenler:** Bir Collider (Is Trigger = true).
- **Inspector Alanları:** 
  - `persona` (NPCPersona): Bu NPC'nin hangi karakteri oynayacağı (ScriptableObject).
- **Mantık:** 
  - `OnTriggerStay` veya `Update` içerisinde Mesafe Kontrolü yapılır. Oyuncu belirli bir mesafedeyse ve "E" tuşuna basarsa sohbet UI'ını açar ve `ChatUIManager`'a kendi `persona`'sını gönderir.

### D. Kullanıcı Arayüzü (Chat UI)
Sohbet sisteminin gözükeceği Unity Canvas arayüzü.

**`ChatUIManager.cs` (MonoBehaviour)**
- **İhtiyacımız Olan UI Elementleri:**
  - **Chat Panel:** Açılıp/Kapanan tüm UI'yi kaplayan Canvas objesi.
  - **Scroll View:** Gelen ve giden mesajların ekranda kaydırılabilir şekilde bulunacağı tarihçe (Chat History).
  - **Message Prefab:** TextMeshPro içeren ufak prefabrik hücreler (Kullanıcı için sağa yaslı mavi arka plan, Yapay Zeka için sola yaslı gri arka planlı olabilir).
  - **Input Field (TMP):** Oyuncunun mesajı yazdığı alan.
  - **Send Button & Model Selection Dropdown (Opsiyonel):** İstek yollama butonu ve çekilen bedava modeller arasından seçim yapılabilen bir dropdown menüsü.
- **Mantık:** 
  - Gelen metinleri ekrana yansıtırken `OpenRouterClient`'ı kullanarak "Gönderiliyor..." (Loading) bildirimleri de göstermelidir.

---

## 3. Sistem İşleyiş Döngüsü (Workflow)

1. **Açılış/Sahne Yüklenmesi:** `OpenRouterClient` internet üzerinden ücretsiz modellerin (Free models) listesini çeker. 
2. **Karakterle Etkileşim:** Oyuncu NPC'nin yanına gelir, E tuşuna basar.
3. **Konuşma Başlangıcı:** `ChatUIManager` görünür olur. `NPCPersona` içerisinden `systemPrompt` alınır ve chat history'ye (gizli bir şekilde) ilk mesaj olan "system" rolüyle eklenir.
4. **Oyuncunun Mesajı:** Oyuncu mesajını yazar -> Mesaj history listesine `role: "user"` olarak eklenir -> UI güncellenir.
5. **API İsteği:** Sistem, UnityWebRequest (IEnumerator / Coroutine) ile OpenRouter'a JSON yollar ve cevabı bekler. Bu sırada UI'da bir *loading* animasyonu/yazısı olur.
6. **API Cevabı:** Gelen veri alınır -> JSON parse edilir -> Asistan mesajı olarak history listesine `role: "assistant"` olarak eklenir -> UI güncellenir.
7. **Çıkış:** Etkileşim bitirilince Panel kapatılır ve sohbet logları NPC bazlı temizlenir veya kayıt altında tutulur.

## 4. Gereksinimler & Kurulumlar
- **TextMeshPro:** UI metinleri için projenizde TextMeshPro import edilmiş olmalıdır.
- **Newtonsoft.Json:** Package Manager üzerinden `com.unity.nuget.newtonsoft-json` yüklenmelidir. OpenRouter cevapları nested (iç içe) olduğundan standart Unity JSON ile çekmesi zordur.
- **Asenkron Yapı:** UnityWebRequest için `IEnumerator` Coroutine veya `async/await` (`Task`) yapıları kullanılacaktır.

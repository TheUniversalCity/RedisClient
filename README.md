**The Cache Technology And High Performance .NET Redis Client With RESP3
Protocol Support**

-   **What Is Cache ?**

Cache is an extremely common software term which has a lot of areas of
usage in our lives and a vital role that makes systems work properly.
Its examples also can be encountered often out of software field. For
instance, preparing the ingredients for a meal or acquiring the
materials supposed to be used in repair serves the same purpose with
creating a cache system for a specific job. The essential point here is
that whatever you keep in order to fulfill whatever you planned must be
available at the closest place to the its point of use. If you store the
ingredients in a cellar or refrigerator instead of the bench that you
prepare the food on, it does not provide convenience and sufficient
speed to you.

We are using these mechanisms in our software career tons of times and
it is quite probable that there are many other fields we are using it
but we did not notice yet. When the cases of using the variables defined
in an ordinary method more than once in their run-time is thought, they
are stored in CPU\'s memory(L3 Cache) because of not applying to RAM
frequently. You can see the CPU\'s are designed as apt to storing and
hold standby the things that they need to carry on a task at their
nearest slot although access speed of RAM is fast enough like an HTTP
request as we disregard its toil.

However, the actual data need to be processed in some cases while the
mutual data of multiple transactions are disscussed. Despite the
perfomance loss occurs, cache mechanism is deactivated in this situation
because of the setup of operations. It is enough to use the
\"**volatile**\" keyword while defining the variables in C# programming
language so that caching usages of variables become prevented. Thus, the
data which have been obtained from every variable access gets brought
through RAM.

-   **Usage of Outer Service Data Over Internet and Caching Process**

As developers, most of the systems we have developed connect each other
through internet. We execute our operations with the data that collected
from many resources on the internet consonant to our profession
principles. If there are some that do not get updated often at their
resources within these data or there is not such obligation to use
current data in the meantime, writing these data to the RAM and using
them through the RAM when the next neccesity occurs make the daha
approach our bench and provide high performance gain we access them next
time.

Caching operations that we have mentioned by this point are basic and
already used technologies without making a great effort. The next need
of cache after this point starts to become a little bit more complex
where we try to access the common data via different applications on the
internet because there is a group of applications in our system and all
of them need to work on the same data. This leads to assuring those
applications work with same data but none of them cannot directly store
the data at their local variables. If they do, then they are not able to
ensure that they are using the same data with other ones.

Here are the two ways to proceed;

-   Accessing the most current data through its resource

-   Accessing the data by placing it to the common and different point
    that closer to the application than its resources after it has
    been obtained from there

Maybe direct access makes sense if the data does not pass through any
transaction and distance of data source is close as any common spot.
However, there is a requirement of a close spot to locate data and that
spot needs to include rapid technologies for the access if adjustment of
writing speed needs an operation at the data source and the data source
is not close enough. This type of systems are called as "**Distributed
Cache Systems**". Redis and Memcached are some example technologies of
the distributed cache systems. They are designed and developed to
achieve high performance for storing and collecting data and at the
transfer protocols.

-   **Caching in Scalable Systems**

To supply with the high traffic occured due to increase of the
importance of internet nowadays, an available application's amount
should be able to increment when required or sources of server that the
application involves in can be enhanced.

-   The operation of enhancing the server sources is called as "**Vertical Scaling**"

-   Increment of the application amount on the same server is called "**Horizontal Scaling**"

Requirement of horizontal scaling increases the application numbers and
its caching transactions need to be managed on multiple applications. In
this case, caching transactions should be executed on distributed cache
systems because data consistency is quite important.

Distributed cache systems are never the closest method to the place
where their own transactions are being executed and therefore their
perfomance must be managed certainly. For example, reaching or updating
the data that is stored on a Redis server substantially consume the
bandwidth when accessing a data is needed thousands of time in a loop
because it is provided through a network transfer. On the other hand,
also data recency would need to be guaranteed if the data was directly
written on the RAM which is one of the nearest place to execution area
for each application.

Some "**Distributed In-Memory Cache**" solutions are produced and used
on horizontal scalable systems in consideration of these problems. The
requested data at the most common solutions should be included together
with the close memory of applications(RAM) and distributed cache at the
mutual spot. Besides, applications listen to a message channel in order
to find out the situations of data recency changes in their close
memories. Other applications that update these related data on the
distributed cache leave the update messages to this message channel.
Furthermore, the applications that receive the update messages remove
the updated data from their close memories and when needed again, they
store those messages as they acquire through the distributed cache in
common spot to reuse for once in their close memories. Hence, the
applications access the data that its update messages have not been
received yet after they had been collected and stored in applications'
close memories with a faster way instead of consuming the bandwidth with
continously applying for the common place on the network.

This method leads to some problems occur with the mechanism it applies
while it is trying to resolve one. Some of those are:

-   Data in distributed cache point are usually stored with a validity
    period(Time to Live or TTL). This period is ignored when the
    collected data is getting placed in the RAM. Data might be deleted
    from distributed cache point if there is not any update message
    sent belongs to it and the application cannot assure the recency
    of data in its close memory in this case.

-   This system has been created for controlling the recency of data
    stored in close memory through a message channel. A change made by
    an application that does not work with the system in distributed
    cache on the common spot or a person manually, does not sent the
    update messages via this message channel. In this situation, the
    application cannot assure the recency of data in its close memory
    as well.

-   **Caching in LCWaikiki Modernization Projects**

We prefer the Redis application for caching at the busy points where we
are applying horizontal scaling in the LCWaikiki projects. Until a few
months ago, we were using the distributed in-memory cache technique at
some points.

By June 2021, we have included LocalizationService application that had
been used by the most of LCWaikiki e-commerce projects in modernization
process. We had preferred the **Stackexchange.Redis** library which we
have used also in previous projects for the Redis connection in
modernization.

While our modernization projects continue, individually I had researches
and studies about some of transfer technologies and I was testing these
technologies on the Redis Client project that I develeoped before from
scratch. In addition, I was trying to develop a web based Redis Web
Manager application like Windows based Redis Desktop Manager
application.

I planned the use **Invalidate Mechanism** that comes with RESP3
protocol after I got inspired from my own research when I was designing
the modernization structure. This mechanism that is needed for
**Distributed In-Memory Cache** system does not support 6.0.0 and later
versions of Redis server.

The reason why I chose this system is avoiding the problems that I have
mentioned above and providing the benefits below:

-   RESP3 protocol can manage the pub-sub mechanism that needs another
    channel at RESP2 through a single channel. This reduces the
    numbers of connection by half.

-   **Client tracking** saves which client has queried the which keyword
    at the last time. No matter how the change happens, it sends
    "invalid" message as "PUSH" object in the protocol to the only
    clients which have used the expired or TTL period ended keyword.

    -  Message traffic exponantially decreases.

    -  With this method, the data source server guarantees the transmission
    of change to the related clients by any means while pub-sub
    messages are handled at application level in other methods.

    -  Invaildity messages handled at application level out ot RESP3 cannot
    guarantee the transmission of TTL status changes. At the same
    time, The invalidity messages of keys changed by the applicalitons
    which are not able to manage at the systems have invalidity
    messages handled at application level won't be sent to the other
    applications. This breaks the consistency of data.

 **Stackexchange.Redis** library supports RESP1 and RESP2 protocol
 versions of RESP but it doesn't support the RESP3. I thought we should
 use the RESP3 technology if we consider the high data traffic that is
 provided by pandemic and technologic developments in addition to the
 high data traffic which comes with the periodic campaigns.

 I talked to our IT director Utku Tatlıdede about using the Redis
 connection provider mechanism that I have already used at my personal
 attempts after making it a package with customizing for using on our
 technology transformation project. He finds its advantages significant
 and leans towards testing it on technology transformation project.

 After the busy points which was using Stackexchanged.Redis struggled
 to response at November campaign, we replaced those points with RESP3
 protocol supported Redis connection provider that I just developed and
 rest of November campaign became quite easy for us.

 Eventually, I have developed a global library and we have proved the
 capabilities of this product with support of our LCWaikiki management.
 You can get the codes via Github address below. Together we can
 improve the capabilities of library and contribute to technology goes
 forward.

 [*https://github.com/orgs/TheUniversalCity/repositories*](https://github.com/orgs/TheUniversalCity/repositories)

-   **The RESP3 Redis Connection Provider Structure**

 Redis transfers the data with RESP application layer protocol over
 TCP/IP protocol. Core knowledge about TCP/IP should be reviewed to
 understand the structure of Redis data transfer protocol called RESP.

 TCP/IP is a transmission protocol that transfers "byte" array between
 two devices bi-directional. A **Full-Duplex** connection channel
 physically opens when a TCP connection is supplied. Data transmission
 can be made two-sided on this connection at the same time and both
 sides communicate each other with this data transmission.

 ![](duplex.jpg)

 Those sides are called "Passive" and "Active" in the TCP definition.

 **Passive Side:**

 One of sides always should open the connection earlier than other one
 to provide a connection. Passive side that is able to open a **port**
 without waiting the other side waits accepting connection request
 after it opens the port. A passive side can be in accord with multiple
 active sides physically at the same time through the port opened by
 itself. It doesn't have to inform the active side sent connection
 request to itself about its tasks because of its listener role.

-   Responses with completing a task according to commands and data
    received from active side

-   Directly provides data transmission to handshaked active side
     without waiting any command or data about its undertaken tasks

 Due to these features of it, passive side is known as "**server**".
 Redis server is the passive side of its provided connection.

 **Active Side:**

 Active side always must provide the connection as handshaked with a
 passive side that its location is known by it. An active side is able
 to handshake physically with an only one passive side. It knows the
 tasks undertaken by passive side which it sent the connection request
 due to its client role and it waits for its request to get
 corresponded and this is why it is known as **client**. The clients
 provide connection to Redis server are the active sides at that
 connection.

 These active and passive sides need to create a language between each
 other for explaining the meaning of data transferred over TCP
 connection and mutual communication. Data can be sent or received
 physically and simultaneously in the duplex transfer protocol such as
 TCP. So, it is unclear when the server will response again for the
 request that sent by the client for the data it will request and it
 needs to be known which response is the response of which request.
 Besides, it also needs to be knows how much of received data is a
 response. All of the standard rules determine how all this complexity
 works and manage the requests and responses in a certain manner,
 create an upper protocol in application level. Also **RESP (REdis
 Serialization Protocol)** is one of these protocols.

-   **RESP (REdis Serialization Protocol)**

 Redis is a server and it supplies with the necessities of clients
 connected to itself as part of a protocol's rules. The definition of
 this protocol is called "Redis Serialization Protocol" and "RESP" is
 the abbreviation of that.

 Essentially, RESP assures the assignment of a unique number to clients
 connected through TCP. While communicating with clients in this way,
 it assures the transmission of commands and their responses which
 received from clients that have an assigned unique number to the
 clients that are in the same queue and same number. This technique is
 called "**Multiplexing**" used often in communication technologies.

 ![](multiplexing.jpg)

 Only one data transfer operation can be executed from a single channel
 physically in the simplex mode at the same time. Separating a lot of
 different messages in the background after they have been transferred
 and combined is named "**Multiplexing**".\
 RESP rules supply the multiplexing operation in Redis communication.
 Data is transferred as a whole, up to serialization rules and ordered
 in the Redis transmission. Also responses are ensured to transfer with
 the same order.

 High speed is the only factor for using queue method in Redis
 multiplexing structure. Parts of a message can be sent also as complex
 with their metadata by using numbering methods belong the messages in
 some other protocols. However, this method needs extra transactions to
 interpret the metadata during splitting and combining the message. So,
 this leads to performance loss occurs. Redis sends the data as
 supplemental and in ordered without using the metadata to gain
 performance and it guarantees the queue which it sent data into is in
 the same order with the command it receives.

 There are some metadata explanations that makes interpreting the data
 which is transmitted in some upper protocols like HTTP and correlate
 it response easier. For instance, also content type is added as
 metadata with "Content-Type" title to make the server knows structure
 of data which is sent in HTTP data transfers. RESP doesn't contain any
 metadata except the data during transmission because it aims high
 speed.

 You can examine the Redis response below:

 \$11\<CR\>\<LF\>Received Response\<CR\>\<LF\>

 This message is transmitted with starting over in order while it is
 being sent to client side as response. Client needs to know the type
 of data and where the transfer ends to decode the response. Therefore,
 data type is sent first in RESP definition. The "\$" symbol above
 states this data is "Bulk String". All the numbers sent between "\$"
 and "\<CR\>\<LF\>" indicates how much byte the data will be. For
 example the number "11" is written above reports 11 bytes should be
 read between first "\<CR\>\<LF\>" and last "\<CR\>\<LF\>". You can
 reach the detailed information via the link below.

[*https://redis.io/topics/protocol*](https://redis.io/topics/protocol)

 This forward informing structure is quite efficient method because
 transferring\
 and interpreting data operations can be made at the same time.\
 \
 Redis supports also operation of subscribing to a message channel
 (PUB/SUB) along with\
 command processing and responding. A client waits for command through
 the server without sending a command when it subscribes to the server
 for a specific message channel in RESP1 and RESP2 descriptions. Hence,
 protocol of opened channel becomes a PUSH protocol because it loses
 its ordered multiplexing feature and consequently it loses its sending
 command and receiving response features too.

 In RESP3 description, multiplexing feature is extended with added new
 object types. Other objects that waited for sent commands in order
 because automatic messages delivered by server are sent with PUSH
 object to client are distinguished and communication continues without
 disarraying their sequence numbers. Thus, also subscribe messages that
 sent via server are obtained from client side without mixing them with
 command responses by the time the responses are getting after sending
 command over the same client channel.

 Client side has become able to support also client cache operations
 due to RESP3 description contains also PUSH notifications too. Because
 in RESP3 description, clients have gained also the feature of
 receiving the notifications of the keys that became invalid and they
 have queried before from the channel that they queried the values
 corresponding to key at te same time thanks to extended PUSH
 notifications.

 When the following command has been transmitted;

 **CLIENT TRACKING on**

 Redis stores related client into **INVALIDATE TABLE**. This table
 records which keys have been queried from which client and if one the
 keys in this table becomes invalid, deleted, updated or expired, it
 sends an **Invalidate Push** like below after it detects the clients
 which have queried for that key.

 \>4\<CR\>\<LF\>\
 +invalidate\<CR\>\<LF\>\
 +key1\<CR\>\<LF\>\
 +key2\<CR\>\<LF\>\
 +key3\<CR\>\<LF\>

 Client that has received this notification removes the data belongs to
 key1, key2 and key3 which it stores in the cache of its own side and
 when a request comes for those keys, it supplies with this request
 over Redis not from its own cache.

 Data that belongs to related client in invalidate table on the Redis
 is deleted too if that client's connection gets closed. In this case,
 the client must delete the all keys involved in its own memory. It
 won't take the invalidate messages belong to the keys that it requests
 at connection anymore because it will get a new number from Redis
 server on the connection that recently created from itself.

 This protocol is carried out concurrent between the client and server.
 You can try the RESP3 protocol with using a client library that brings
 these features to the system.

-   **TheUniversalCity.RedisClient**

[*https://github.com/orgs/TheUniversalCity/repositories*](https://github.com/orgs/TheUniversalCity/repositories)

 **TheUniversalCity.RedisClient** developed in .Net Standart Library
 can be used on the application types below.

  | **.NET Application**             |   **Version Support**  |
 | -----------------------------     |  ------------------------------------------ |
 | .NET ve .NET Core                 |  2.0, 2.1, 2.2, 3.0, 3.1, 5.0, 6.0   |
 | .NET Framework                    |  4.6.1, 4.6.2, 4.7.0, 4.7.1, 4.7.2, 4.8.0   |                                 
 |  Mono                             |  5.4, 6.4    |    
 |  Xamarin.iOS                      |  10.14, 12.16    |
 |  Xamarin.Mac                      |  3.8, 5.16   |
 |  Xamarin.Android                  |  8.0, 10.0   |
 |  Universal Windows Platform       |  10.0.16299, TBD |
 |  Unity                            |  2018,1  |

-   **Client Create and Connection**

 **RedisClient** class includes a static **CreateClientAsync** method.
 RedisClient object is created and connection is provided with Redis by
 using this method. This method can be overridden in two different
 ways.

 "var client = RedisClient.CreateClientAsync(\_connectionString)"

 and

 "var client = RedisClient.CreateClientAsync(new RedisConfiguration())"

 "**\_connectionString**" is a string expression. It is a clause that
 neccessary settings for connection are initialized. At least one IP
 address or host value is mandatory. It is used with the structure
 below.

 "**\[ipAddress\|host\]\[:\[port\]\],
 \[ipAddress2\|host2\]\[:\[port\]\],\[optionKey\]=\[value\]
 ,\[optionKey2\]=\[value\]**"

 Example: "localhost,clientCache=true"

   **Options**

 The options and their explanations can be used in connection clauses
 below:

 | **Option**            |  **Description**     |     **Default**   |
 | -----------------     |  ----------------    |  ---------------  |   
 | clientCache  |    Activates client-side cache property. Gets "true" and "false" values. Redis version must be greater than 6.0.0 to assign "true" value.     |   false   |
 | password     |   If exists, it takes password value for Redis connection.    |   null   |
 | db   |   Indicates database number for Redis connection.     |   0   |            
 | connectRetry    |  Restarts the connection attempts after is case of it tries for all IP addresses and getting no connection still.  |   3   |
 | connectRetryInterval     |   States how much ms later new attempt will start after each connection attempt.  |   300    |           
 | receiveBufferSize    |   Sets size of receive buffer storage for TCP socket.    |   65536   |       
 | sendBufferSize     |   Sets size of send buffer storage for TCP socket.     |   65536   |        

-   **ExecuteAsync(cancellationToken, param1, param2\...) Method**

 There is a provided TCP/IP connection between client and Redis server
 when a RedisClient object is created. All asynchronous requests are
 sent over this method except urgent messages sent from client to
 server. There are two different ways of its usage. In one of them,
 parameters can be sent as string while they are sent as byte\[\] in
 the other one. Each sent parameters are sent to server as command
 segments.

 **ExecuteAsync** method lines up the requests due to its structure and
 executes transfer operations as a whole after it writes parallel
 requests which are in transmission queue under high traffic to the
 buffer storage. It supplies acquiring high performance during data
 transfer.

 **NOTE: GetAwaiter().GetResult()** certainly must not be used for
 getting the result. This method already tends to start parallel
 transaction as far as possible because parallel transactions are
 optimized in data writing operations. When trying to get the result
 through **GetAwaiter().GetResult()** or directly **Result** property,
 an extreme slowdown occurs because it struggles to finalize the
 response on same thread. So, it should be used in Async/Await methods
 always.

 It returns the response as a single "Task" object. When the response
 is returned over Redis server, "Task" object becomes completed status
 and the returned value is acquired.

 Its usages are shown as below:

 string value = await ExecuteAsync(cancellationToken, "GET", "key");

 string value = await ExecuteAsync(cancellationToken, byteArray1,
 byteArray2);

-   **Execute(cancellationToken,param1,param2\...) Method**

 It has the same function with ExecuteAsync method but it doesn't
 include asynchoronous transaction optimization.

 It returns the response as a one "string" object when a response is
 returned over Redis server.

 Its usage are shown as below:

 string value = Execute(cancellationToken, "GET", "key");

 string value = Execute(cancellationToken, byteArray1, byteArray2);
-   **ExecuteEmergencyAsync(param1,param2\...) Method**

 Like ExecuteAsync method, it is responsible to send command and data
 to server. However, the commands that are going to be sent are the
 urgent methods which should be taken to forefront without waiting
 their turns come. Therefore, the message that is supposed to be sent
 is transmitted immediately to the server at one time without caring
 about performance.

 Usually it is used for sending related commands for
 "**db,password,clientCache**"\  settings after first connection has been opened.

 Emergency case starter(**BeginEmergency()**) should be used so that
 this method becomes able to be used.

 Nonurgent channel is stopped in order to this transmission is
 executed. Engagements of two channel at the same time is prevented
 with Flip-Flop method.

-   **GetAsync\<T\>(string key, CancellationToken cancellationToken=default) Method**

 It acquires the data belongs to given key over Redis and returns the
 object created with deserializing in T type. It stores the data that
 it returns in client side if client-side cache property is active.
 When the key is requested again, it controls the key in client cache
 and then it queries for that key's value over Redis if the key isn't
 in there.

 The cancellationToken object which is given as second parameter will
 cancel the Redis operation in necessary cases. "Task Canceled"
 exception is thrown when the Redis operation is cancelled.

 Usage of this method is shown below:

 T value = await GetAsync\<T\>("KEY", cancellationToken);

-   **GetAsync(string key, CancellationToken cancellationToken = default) Method**

 It returns the data as string after it obtains the data belongs to
 given key over Redis.\
 Like GetAsync\<T\>, also this method stores the data that it returns
 in client side if client-side cache property is active and when the
 key is requested again, it controls the key in client cache and then
 it queries for that key's value over Redis if the key isn't in there.

 The cancellationToken object which is given as second parameter will
 cancel the Redis operation in necessary cases. "Task Canceled"
 exception is thrown when the Redis operation is cancelled.

 Its usage is shown below:

 string value = await GetAsync("KEY", cancellationToken);

-   **SetAsync\<T\>(string key, T data, TimeSpan? expiry, CancellationToken cancellationToken = default) Method**

 It uses the data with given key for writing operation on Redis. It
 writes the data in T type as JSON string to Redis by serializing.

 The cancellationToken object which is given as second parameter will
 cancel the Redis operation in necessary cases. "Task Canceled"
 exception is thrown when the Redis operation is cancelled.

 Usage of this method is shown below:

 bool success = await SetAsync\<T\>("KEY", data, TimeSpan.FromSeconds(100), cancellationToken);

-   **SetAsync(string key, string data, TimeSpan? expiry, CancellationToken cancellationToken = default) Method**

 Like its generic version as well, also this method uses the data with
 given key for writing operation on Redis. It writes the data in T type
 as JSON string to Redis by serializing.

 The cancellationToken object which is given as second parameter will
 cancel the Redis operation in necessary cases. "Task Canceled"
 exception is thrown when the Redis operation is cancelled.

 Its usage is shown below:

 bool success = await SetAsync("KEY", data, TimeSpan.FromSeconds(100), cancellationToken);

-   **ClearAsync(CancellationToken cancellationToken, params string\[\] keys) Method**

 It clears the given keys on Redis and returns how many it has cleared.

 The cancellationToken object which is given as second parameter will
 cancel the Redis operation in necessary cases. "Task Canceled"
 exception is thrown when the Redis operation is cancelled.

 The usage of method is shown below:

 long deleteCount = await ClearAsync(cancellationToken, "KEY1", "KEY2","KEY3");


--------------------------------------------------------------------------------------------------------------


## Önbellekleme(Cache) Teknolojileri ve RESP3 Protokol Destekli Yüksek Performanslı .Net Standart Redis Client

-   **Cache(Önbellek) Nedir?**

Yazılım terimleri arasında çok sık kullandığımız “Cache” yani “Önbellek”
hayatımızın her noktasında kullandığımız ve sistemlerin çalışmasında
hayati önem taşıyan bir terimdir. Yazılım sektörü dışında da örneklerine
sık rastlayabileceğimiz konulardandır. Örneğin bir yemek yapımında ya da
bir tamirat işleminde kullanılması planlanan malzemelerin tezgâh
üzerinde hazır olarak bulundurulması da bir önbellek oluşturulması ile
aynı işlevi görür. Burada temel nokta şudur; her ne yapmak için hazır
olarak bulunduracağınız her ne ise kullanım noktasına en yakın yerde
bulundurulmalıdır. Yani yemek yapımı için hazırda bulundurmanız gereken
materyalleri yemeği yaptığınız tezgâh yerine kilerde ya da dolapta
hazırda bekletirseniz, size yemek yapımı sırasında yeterli kolaylığı ve
hızı kazandırmayacaktır.

Yazılım meslek hayatımızda bu mekanizmaları o kadar yoğun kullanıyoruz
ki kullandığımızı fark etmediğimiz birçok nokta olması muhtemeldir.
Örneğin sıradan bir metot içinde kullandığımız değişkenler, çalışma
zamanında metot içinde birden fazla kullanıldığı durumlar düşünülerek
RAM üzerine sürekli başvurmamak için işlemcinin belleğine alınırlar (L3
bölgesi). RAM erişim hızı bir http çağrısına kıyasla bizim hiç külfetini
dikkate almayacağımız kadar hızlı olsa bile işlemcinin işlem yapmak için
ihtiyacı olan her ne ise malzemelerini en yakın bölgesinde hazırda
bekletme eğiliminde tasarlandığını görürsünüz.

Ancak bazı durumlarda çoklu işlemlerin paylaştığı ortak veriler söz
konusu olduğunda en güncel verinin işlenmesi gerekebilir. Bu durumda her
ne kadar hız kaybı yaşanacak olsa da iş kuralı gereği önbellek
mekanizması devre dışı bırakılır. İşlemci örneğinde değişkenlerin
önbellek kullanımlarını engellemek için C\# dilinde değişkenler
tanımlarken **“volatile”** anahtar kelimesinin kullanılması yeterlidir.
Bu sayede her değişken erişiminde elde edilen veri RAM üzerinden
getirilir.

-   **İnternet Üzerinden Kullanılan Dış Servis Verileri ve Önbellekleme
    İşlemi**

Yazılımcılar olarak geliştirdiğimiz çoğu sistem internet üzerinden
birbirine bağlanmaktadır. İş kurallarımız gereği internet üzerindeki
birçok kaynaktan elde edilen veriler üzerinden işlemlerimizi yürütürüz.
Eğer bu veriler içinde kaynağında çok sık değişmeyenler varsa veya
değiştiği zaman anlık olarak güncel verinin kullanılması zorunlu
değilse, bu verileri ilk servis çağrısı sonrası RAM üzerine yazmak ve
bir sonraki ihtiyaçta RAM üzerinden kullanmak, veriyi tezgahımıza
yaklaştırmak olacaktır ve yeniden erişimimizde bize yüksek performans
kazanımı sağlayacaktır.

Bu noktaya kadar bahsettiğimiz önbellekleme işlemleri temel ve
halihazırda çok çaba sarf etmeden kullandığımız teknolojilerdir. Bu
noktadan sonraki ihtiyaç, aynı ortak veriye internet üzerindeki farklı
uygulamalardan erişmeye çalıştığımız noktada biraz daha karmaşıklaşmaya
başlıyor. Çünkü burada sistemimizde bir grup uygulama var ve hepsinin
aynı veri üzerinde çalışması gerekiyor. Bu da ortak bir noktadan eşit
güncel veri ile çalışmalarını garanti etmek demektir ve bu durumda her
uygulama doğrudan kendi lokal değişkenlerinde bu veriyi saklayamaz. Eğer
saklarlarsa diğer uygulamalarla birlikte aynı veriyi kullandığını
garanti edemezler.

Burada iki yöntemle ilerlenebilir;

-   Birincisi, en güncel veriye kaynağından erişmek,

-   İkincisi ise veriyi kaynağından alıp, uygulamalara kaynağından daha
    yakın ortak başka bir noktaya koyarak erişmek.

Eğer veri herhangi bir işlemden geçmiyor ve veri kaynağının yakınlığı
uygulamalar için kullanılabilir herhangi bir ortak nokta kadar yakınsa
doğrudan kaynağından erişmek mantıklı olabilir. Fakat okuma yazma hızı
verinin kaynağında işlem gerektiriyor ve yakınlık anlamında da yeterince
yakın değilse, veriyi konumlandırmamız gereken yakın bir nokta ve bu
noktanın da erişimde hızlı teknolojiler içermesi gerekiyor. Bu tip
sistemlere **“Dağıtık Önbellek Sistemleri(Distributed Cache Systems)”**
adı veriliyor. Örnek olarak Redis ya da Memcached uygulamaları bu tip
dağıtık önbellekleme sistemleri sunan ürünlerden bazılarıdır. Veri
saklama elde etme ve transfer protokollerinde yüksek performans
hedeflenerek tasarlanmışlardır.

-   **Ölçeklenebilir Sistemlerde Önbellekleme İşlemi**

Günümüzde internetin öneminin artması ile oluşan yoğun trafiği
karşılayabilmek adına, sunulan bir uygulamanın gerektiğinde şartlara
göre sayısının artırılabilmesi ya da içinde bulunduğu sunucunun
kaynaklarının artırılabilmesi gerekmektedir.

-   Sunucu kaynaklarının artırılması işlemine **“Dikey Öçeklendirme
    (Vertical Scaling)”** ,

-   Aynı sunucu kaynakları üzerinde uygulama sayısının artırılması ise
    **“Yatay Ölçeklendirme (Horizontal Scaling)”** adı veriliyor.

Yatayda ölçeklendirme ihtiyacı, uygulama sayılarını artırır ve
önbellekleme işlemlerinin birden fazla uygulamada yönetilmesi gerekir.
Bu durumda veri tutarlılığının önemli olmasından dolayı önbellek
işlemlerinin dağıtık önbellekleme sistemleri üzerinden yapılması
gerekebilir.

Dağıtık önbellekleme sistemleri hiçbir zaman işlemlerin
gerçekleştirildiği noktaya en yakın yöntem değildir. Bu yüzden bir
noktada performansın mutlaka yönetilmesi gerekir. Örneğin Redis sunucusu
üzerinde tutulan bir veriye ulaşmak veya o veriyi değiştirmek ağ
transferi üzerinden sağlandığı için bir döngüde binlerce kez bir veriye
erişim ihtiyacı duyulduğunda ağ bant genişliğini ciddi oranda
tüketecektir. Ancak diğer yönden de veriyi doğrudan her uygulama için
işlem bölgesine en yakın yerlerden biri olan RAM bölgesine yazmış
olsaydık her bir uygulama için verinin güncelliğini de garanti altına
almamız gerekirdi.

Bu problemler ışığında yatay ölçeklenebilir sistemlerde bazı **dağıtık
yakın hafıza önbellekleme (Distributed InMemory Cache)** çözümleri
üretilmiş ve kullanılmaktadır. En yaygın çözümde erişilmek istenen veri,
uygulamaların yakın hafızası (RAM) ve ortak bir noktadaki dağıtık
önbellek (Distributed Cache) noktalarında birlikte barındırılmaktadır.
Uygulamalar yakın hafızalarındaki verilerin güncelliğinin değiştiği
durumları öğrenmek için ayrıca bir mesaj kanalı dinlerler. İlgili
verileri dağıtık önbellek üzerinde güncelleyen başka uygulamalar ise bu
mesaj kanalına değişim mesajlarını bırakırlar. Değişim mesajlarını alan
uygulamalar yakın hafızalarındaki değişen verileri silerler ve ihtiyaç
duyduklarında tekrardan tek seferlik olarak ortak noktadaki dağıtık
önbellek üzerinden elde ederek tekrar kullanmak üzere yakın hafızalarına
kaydederler. Böylelikle uygulamalar sürekli ağ üzerindeki ortak noktaya
başvurarak bant genişliği tüketmek yerine, en son elde edildikten sonra
değişim mesajı gelmemiş yakın hafızalarına aldıkları veriyi kullanarak
veriye daha hızlı bir yöntemle ulaşmış olurlar. Değişim mesajları ile de
verinin güncelliğini sağlamış olurlar.

Bu yöntem bir problemi çözmeye çalışırken uyguladığı mekanizma ile bazı
problemleri de ortaya çıkartır. Bunlardan bazıları şunlardır:

-   Dağıtık nokta üzerinde önbelleğe alınan veriler genellikle bir
    geçerlilik süresi ile kayıt altına alınırlar (Time to Live.
    Bkz: TTL). Yakın hafızaya konumlandırılan veri elde edilirken bu
    süre gözetilmeden kayıt altına alınır. Eğer bu geçerlilik süresi
    içinde veriye ait bir değişim mesajı gönderilmemişse veri dağıtık
    önbellek noktasında silinmiş olabilir. Bu durumda uygulama yakın
    hafızasında bulunan verinin güncelliğini garanti edemez.

-   Bu sistem, yakın hafızaya alınmış verinin güncelliğinin bir mesaj
    kanalı üzerinden kontrol edilmesi üzerine kurulmuştur. Ortak
    noktadaki dağıtık önbellek üzerinde bu sistemle çalışmayan bir
    uygulama tarafından yapılan ya da insan eliyle yapılacak bir
    değişiklik, bu mesaj kanalı vasıtası ile değişim
    mesajları iletmeyecektir. Bu durumda uygulama yakın hafızasında
    bulunan verinin güncelliğini garanti edemez.

-   **LCWaikiki Teknoloji Dönüşüm Projelerinde Önbellekleme**

LCWaikiki uygulamalarında yatay ölçekleme yaptığımız yoğun noktalarda
önbellekleme için Redis uygulamasını tercih ediyoruz. Birkaç ay öncesine
kadar bazı noktalarda yukarıda bahsettiğim dağıtık yakın hafıza
önbellekleme tekniğini kullanıyorduk.

Haziran 2021 itibari ile LCWaikiki E-Ticaret ailesinde kullanılan
LocalizationService uygulamasını teknoloji dönüşümüne dahil ettik. Redis
bağlantısı için modernizasyonda kullanmak üzere daha önceki projelerde
de kullandığımız **Stackexchange.Redis** kütüphanesini tercih etmiştik.

Teknoloji dönüşüm projelerimiz devam ederken bireysel olarak bazı
transfer teknolojileri üzerinde de araştırma ve çalışmalarım vardı ve bu
teknolojileri sıfırdan geliştirdiğim bir Redis Client uygulaması
üzerinde uygulayarak denemeler yapıyordum. Bu çalışma ile Windows
tabanlı Redis Desktop Manager uygulaması benzeri Web tabanlı Redis Web
Manager uygulaması geliştirmeye çalışıyordum.

Dönüşüm mimarisini tasarlarken kişisel araştırmalarımdan esinlenerek
RESP3 protokolü ile gelen geçersizlik mekanizmasını (**Invalidate
Mechanism**) kullanmayı planladım. Dağıtık yakın hafıza
önbellekleme(**Distributed InMemory Cache**) sistemi için gerekli olan
bu mekanizma, Redis sunucusu versiyonlarından 6.0.0 versiyonu ve
sonrasında desteklenmektedir.

Bu sistemi tercih etme sebebim yukarıda bahsettiğim problemlerden
kaçınmak ve aynı zamanda aşağıdaki faydaları sağlayabilmekti:

-   RESP3 protokolü, RESP2 de ayrıca başka bir kanal gerektiren pub-sub
    mekanizmasını tek kanaldan yönetebiliyor. Bu da bağlantı sayılarını
    yarı yarıya düşürüyor.

-   RESP3 protokolünde istemcileri takip etme mekanizması
    **(client tracking)** hangi istemcinin en son hangi anahtar kelimeyi
    sorguladığını tutuyor. Hangi şekilde değişiklik gerçekleşirse
    gerçekleşsin, değişen ya da TTL süresi dolan anahtar kelimeyi sadece
    hangi istemciler kullanmışsa onlara protokolde geçen “PUSH” nesnesi
    olarak “geçersiz” mesajı iletiyor.

    -   Mesaj trafiği üssel derecede azalıyor.

    -   Diğer yöntemlerde pub-sub mesajları uygulama seviyesinde
        yönetilirken, bu yöntemde veri kaynağı olan sunucu her ne
        şekilde olsun değişimin ilgili istemcilere iletilmesini
        garanti ediyor.

    -   RESP3 dışındaki uygulama seviyesinde yönetilen geçersizlik
        mesajları TTL durum değişikliklerini iletmeyi garanti edemez.
        Aynı zamanda uygulama seviyesinde yönetilen geçersizlik
        mesajları içeren sistemlerde yönetimi yapmayan uygulamaların
        değiştirdiği anahtarların geçersizlik mesajları diğer
        uygulamalara iletilemeyecektir. Bu da veri tutarlılığını bozar.

**Stackexchange.Redis** kütüphanesi Redis sunucusunun iletimde
kullandığı RESP protokol versiyonlarından RESP1 ve RESP2 yi
destekliyorken RESP3 protokolünü desteklememektedir. Pandemi ve
teknolojik gelişmelerin desteklediği yüksek veri trafiğine ek olarak bir
de dönemsel kampanyaların getireceği yüksek veri trafiğini de ele
aldığımızda RESP3 teknolojisini mutlaka kullanmamız gerektiğini
düşündüm.

Halihazırda bireysel olarak denemelerimde kullandığım Redis bağlantı
sağlayıcısı mekanizmasını özelleştirerek paket haline getirip dönüşüm
projemizde kullanmak üzere direktörümüz Utku Tatlıdede ile görüştüm.
Kendisi de avantajlarını dikkate değer buldu ve dönüşüm projesinde
deneme konusuna sıcak baktı.

Kasım kampanyalarında Stackexchange.Redis kullanan yoğun yerler cevap
vermekte zorlanınca o noktaları RESP3 protokol destekli yeni
geliştirdiğim Redis bağlantı sağlayıcısı ile değiştirdik. Ve kasım
kampanyasını oldukça rahat bir şekilde geçirdik.

Sonuç olarak evrensel bir kütüphane geliştirmiş oldum ve LCWakiki
yönetimimizin desteği ile de ortaya çıkan ürünün yeteneklerini ispat
etmiş olduk. Kodlarına aşağıdaki github adresinden erişebilirsiniz.
Sizlerin de desteği ile kütüphanenin yeteneklerini daha da ileriye
taşıyarak teknolojiye el birliği ile katkıda bulunmuş olacağız.

[*https://github.com/orgs/TheUniversalCity/repositories*](https://github.com/orgs/TheUniversalCity/repositories)

-   **RESP3 Redis Bağlantı Sağlayıcısı Yapısı**

Redis veri transferini TCP/IP transfer protokolü üzerinden RESP uygulama
katmanı protokolü ile yapmaktadır. Redis’in veri transferi protokolü
olan RESP yapısını anlayabilmek için TCP/IP protokolü hakkında temel
bilgilerin gözden geçirilmesi gerekmektedir.

TCP/IP, iki cihaz arasında çift yönlü “byte” dizisi transferi yapan
iletim protokolüdür. Bir TCP bağlantısı sağlandığında fiziksel olarak
çift yönlü **(Full-Duplex)** bir bağlantı kanalı açılır. Bu bağlantı
üzerinde birbirinden bağımsız iki yönde de aynı anda veri iletimi
yapılabilir. Bu veri iletimi ile iki taraf birbiri ile haberleşmiş olur.

![](duplex.jpg)

TCP tanımında taraflar “Aktif” ve “Pasif” olarak adlandırılırlar.

**Pasif Taraf:**

Bir bağlantının sağlanabilmesi için her zaman bir tarafın bağlantıyı
önceden açması gerekir. Bu görevi pasif taraf üstlenir. Karşısında bir
taraf beklemeden bir kapı **(Port)** açabilen pasif taraf, kapıyı
açtıktan sonra bağlantı taleplerini kabul etmeyi bekler. Bir pasif
taraf, açmış olduğu kapıdan fiziksel olarak aynı anda birden fazla aktif
taraf ile el sıkışabilir. Dinleyici rolü sebebiyle kendisine bağlantı
isteği atan aktif tarafın görevlerini bilmek zorunda değildir.

-   Aktif taraftan aldığı komut ve verilerle bir görev yerine getirerek
    yanıt döner.

-   Üstlendiği görevlere ilişkin herhangi bir komut ya da veri
    beklemeden doğrudan el sıkıştığı aktif tarafa veri
    iletimi sağlayabilir.

Bu özellikleri nedeni ile **“sunucu (Server)”** olarak bilinir. Redis
sunucusu kurduğu bağlantılarda pasif taraftır.

**Aktif Taraf:**

Aktif taraf her zaman konumunu bildiği bir pasif taraf ile el sıkışarak
bağlantıyı sağlamak zorundadır. Bir aktif taraf sadece bir pasif taraf
ile fiziksel olarak el sıkışabilir. İstemci rolü nedeni ile bağlantı
isteği attığı pasif tarafın üstlendiği görevleri bilir ve isteklerinin
karşılanmasını bekler. Bu nedenle **“istemci (Client)”** adı ile
bilinir. Redis sunucusuna bağlantı kuran istemciler kurdukları
bağlantıda aktif taraftırlar.

Bu aktif ve pasif taraflar TCP bağlantısı üzerinden iletilen verileri
anlamlandırmak ve karşılıklı haberleşebilmek için kendi aralarında bir
dil geliştirmeleri gerekir. TCP gibi çift taraflı (Duplex) transfer
protokollerinde fiziksel olarak aynı anda veri gönderilebilir ve
alınabilir. Böylelikle de istemci bir talepte bulunacağı veriyi
ilettiğinde sunucunun tekrar o isteğin cevabını ne zaman döneceği belli
değildir. Ve hangi cevabın hangi isteğin cevabı olduğunu bilmek gerekir.
Ayrıca alınan verinin ne kadarının bir cevap olduğunu da bilmek gerekir.
Tüm bu karmaşanın nasıl işleyeceğini belirleyen ve istekler ile
cevapların belirli bir düzende yönetilmesini sağlayan standart
kuralların tamamı uygulama seviyesinde bir üst protokol oluşturur.
**RESP (REdis Serialization Protocol)** de bu protokoller arasındadır.

-   **RESP (REdis Serialization Protocol)**

Redis bir sunucudur ve kendisine bağlanan istemcilerin ihtiyaçlarını bir
protokolün kuralları çerçevesinde karşılar. Bu protokol tanımına
**“REdis Serialization Protocol”** denilmektedir. “RESP” bu ifadenin
kısaltılmış halidir.

RESP en temelde Redis sunucusuna TCP ile bağlanan istemcilere benzersiz
bir numara atanmasını garanti eder. Bu şekilde istemcilerle iletişim
kurarken atanmış bir numarası bulunan istemcilerden gelen komutların
cevaplarının aynı sırada aynı numaradaki istemciye iletilmesini garanti
etmektedir. Bu tekniğe iletişim teknolojilerinde sıklıkla kullanılan
**“Çoğullama (Multiplexing)”** denilmektedir.

![](multiplexing.jpg)

Fiziksel olarak tek kanaldan yapılan veri transferlerinde aynı anda bir
yönde yalnızca bir veri transferi yapılabilir. Bir çok farklı mesajın
birleştirildikten sonra transfer edilip arkada ayrıştırılması işlemine
**“çoğullama (Multiplexing)“** adı verilmektedir. Redis iletişiminde
çoğullama işlemini RESP kuralları sağlar. Redis iletiminde veriler
serileştirme kurallarına göre bütün halinde ve sıralı bir şekilde
iletilir. Cevapların da aynı sıra ile iletileceği garanti edilir.

Redis çoğullama yapısında sıra yönteminin kullanılmasındaki tek etmen
yüksek hızdır. Diğer bazı protokollerde mesajlara ait numaralandırma
yöntemleri kullanılarak bir mesajın parçaları mesaja ait numara üst
bilgisi ile karışık halde de iletilebilmektedir. Ancak bu yöntem,
mesajın parçalanması ve birleştirilmesi sırasında üst bilgilerin
yorumlanması gibi ek işlemler gerektirmektedir. Bu da performans kaybına
sebep olmaktadır. Redis performans kazanmak için bu üst bilgileri
kullanmadan veriyi bütünler halinde sıra ile gönderir ve gönderdiği
sıranın aldığı komut sırasında olduğunu garanti eder.

HTTP gibi bazı üst protokollerde iletilen verinin yorumlanması veya
cevapla ilişkilendirmeyi kolaylaştıracak bazı üstbilgi(metadata)
tanımları bulunur. Örneğin HTTP veri transferlerinde iletilen verinin
yapısını sunucunun tanıyabilmesi için “Content-Type” başlığı ile
içeriğin tipi de üst bilgi olarak eklenir. RESP iletimde yüksek hızı
hedeflediğinden iletim sırasında veri dışında hiçbir
üstbilgiyi(metadata) barındırmaz.

Aşağıdaki redis cevabını inceleyebilirsiniz:

\$11&lt;CR&gt;&lt;LF&gt;Gelen Cevap&lt;CR&gt;&lt;LF&gt;

Bu mesaj istemci tarafına cevap olarak iletilirken baştan başlayarak
sıra ile iletilir. İstemcinin bu cevabı çözebilmesi için verinin tipini
ve transferin nerede son bulacağını bilmesi gerekir. Bu yüzden RESP
tanımında ilk önce verinin tipi iletilir. Yukarıdaki “\$” harfi bu
verinin “Bulk String” olduğunu belirtir. “\$” ile “&lt;CR&gt;&lt;LF&gt;”
arasında gönderilen tüm rakamlar verinin boyutunun kaç bayt olacağını
belirtir. Yukarıda görünen 11 sayısı ilk “&lt;CR&gt;&lt;LF&gt;” ile son
“&lt;CR&gt;&lt;LF&gt;” arasında 11 bayt okuması gerektiğini bildirir.
Detaylı bilgiye aşağıdaki bağlantı üzerinden ulaşabilirsiniz.

[*https://redis.io/topics/protocol*](https://redis.io/topics/protocol)

Bu önden bilgilendirme yapısı ile veri transfer edilirken aynı zamanda
yorumlama işlemi de yapılabildiğinden oldukça verimli bir yöntemdir.

Redis komut işleme ve cevap göndermenin yanında bir mesaj kanalına
abonelik işlemini (PUB/SUB) de destekler. RESP1 ve RESP2 tanımlarında
bir istemci sunucuya belli bir mesaj kanalı için abone olduğunda komut
göndermeksizin sunucu üzerinden komut bekler. Bu nedenle sıralı
çoğullama özelliğini kaybedeceği için açılan kanalın protokolü PUSH
protokolüne dönüşmüş olur. Dolayısı ile komut gönderme ve cevap alma
yeteneğini kaybeder.

RESP3 tanımında ise çoğullama yeteneği eklenen yeni nesne türleri ile
genişletilmiştir. Sunucunun gönderdiği otomatik mesajları istemciye yeni
tanımla gelen PUSH nesnesi ile gönderildiği için gönderilen sıralı
komutlar için beklenen diğer objeler ayırt edilir ve sırası
karıştırılmadan iletişim devam eder. Bu sayede aynı istemci kanalı
üzerinden komut gönderilip cevapları alınırken aynı zamanda sunucu
tarafından gönderilen abonelik mesajları da istemci tarafından komut
cevapları ile karıştırılmadan elde edilmiş olur.

RESP3 tanımı PUSH bildirimlerini de barındırdığından dolayı istemci
tarafı önbellekleme (Client Cache) işlemlerini de destekler haline
gelmiştir. Çünkü RESP3 tanımında genişletilmiş PUSH bildirimleri
sayesinde istemciler, anahtara karşılık gelen değerleri sorguladığı
kanaldan aynı zamanda daha önce sorgulamış olduğu anahtarlardan geçersiz
hale gelenlerin bildirimlerini de alabilme yeteneği kazanmıştır.

Aşağıdaki komut sunucuya iletildiğinde;

**CLIENT TRACKING on**

Redis ilgili istemciyi **geçersizlik tablosuna (INVALIDATE TABLE)**
kaydeder. Bu tablo hangi anahtarın hangi istemci tarafından
sorgulandığını kayıt altında tutar. Eğer bu tabloda bulunan
anahtarlardan birisi geçersiz hale gelirse, yani silinir, güncellenir ya
da yaşam süresi son bulursa, geçersizlik tablosu üzerinde hangi
istemcinin bu anahtarı sorguladığını tespit edip bu istemcilere
aşağıdaki gibi bir **“Geçersizlik Bildirimi (Invalidate Push)”**
gönderir.

&gt;4&lt;CR&gt;&lt;LF&gt;

+invalidate&lt;CR&gt;&lt;LF&gt;

+key1&lt;CR&gt;&lt;LF&gt;

+key2&lt;CR&gt;&lt;LF&gt;

+key3&lt;CR&gt;&lt;LF&gt;

Bu bildirimi alan istemci, kendi tarafındaki önbellekte sakladığı key1,
key2 ve key3 e ait verileri siler ve yeniden bu anahtarlar için bir
talep gelirse bu talebi önbelleğinden değil Redis üzerinden karşılar.

Eğer bir istemcinin bağlantısı sonlanırsa ilgili istemciye ait Redis
üzerindeki geçersizlik tablosundaki veriler de silinir. Bunun yaşanması
durumunda istemci kendi belleğine aldığı tüm anahtarları silmelidir.
Artık yeni kuracağı bağlantıda Redis sunucusu tarafından kendisine yeni
bir numara verileceği için önceki bağlantıda talep ettiği anahtarlara
ait geçersizlik mesajlarını alamayacaktır.

Bu protokol istemci ve sunucu arasında eşzamanlı yürütülmektedir. Bu
yetenekleri sisteme kazandıran bir istemci kütüphanesi kullanarak RESP3
protokolünü deneyebilirsiniz.

-   **TheUniversalCity.RedisClient**

[*https://github.com/TheUniversalCity/RedisClient*](https://github.com/TheUniversalCity/RedisClient)

.Net Standart Library üzerinde geliştirilmiş olan
**TheUniversalCity.RedisClient** aşağıdaki uygulama türlerinde
kullanılabilir.

  | **.NET uygulama**               |   **Sürüm desteği**  |
  | ----------------------------    |   ------------------------------------------  |
  | .NET ve .NET Core               |   2.0, 2.1, 2.2, 3.0, 3.1, 5.0, 6.0   |
  | .NET Framework                  |   4.6.1, 4.6.2, 4.7.0, 4.7.1, 4.7.2, 4.8.0    |
  | Mono                            |   5.4, 6.4    |
  | Xamarin.iOS                     |   10.14, 12.16    |
  | Xamarin.Mac                     |   3.8, 5.16   |
  | Xamarin.Android                 |   8.0, 10.0   |
  | Evrensel Windows Platformu      |   10.0.16299, TBD |
  | Unity                           |   2018,1  |

-   **İstemci Yaratma ve Bağlantı**

**RedisClient** sınıfı **static** bir **CreateClientAsync** metodu
içerir. Bu metod kullanılarak yeni bir RedisClient nesnesi yaratılır
ve Redis ile bağlantı kurulur. Bu metodun iki farklı aşırı
yüklemesi(**override**) bulunmaktadır.

var client = RedisClient.CreateClientAsync(\_connectionString)

ve

var client = RedisClient.CreateClientAsync(new RedisConfiguration())

**“\_connectionString”** string bir ifadedir. Bağlantı için gerekli
ayarların verildiği bağlantı cümleciğidir. Aşağıdaki yapıda
kullanılmaktadır. En az bir IP adresi ya da host değeri zorunludur.

**“\[ipAddress|host\]\[:\[port\]\],\[ipAddress2|host2\]\[:\[port\]\],\[optionKey\]=\[value\],\[optionKey2\]=\[value\]”**

Örnek: “localhost,clientCache=true”

-   **Ayarlar (Options)**

Bağlantı cümleciği içerisinde kullanılabilecek olan ayarlar ve
açıklamaları aşağıdadır.

  | **Ayar (Option)**   |   **Açıklama**    |   **Varsayılan**  |
  | -----------------   |   ------------    |   --------------  |
  | clientCache |   İstemci taraflı önbellek özelliğini aktif eder. “true” ve “false” değerleri alır. “true” değeri verebilmek için Redis versiyonunun 6.0.0 ve üzerinde olması gereklidir. |   false   |
  | password    |   Redis bağlantısı için eğer varsa şifre değerini alır    |   null    |
  | db  |   Redis bağlantısı için veritabanı numarasını belirtir.   |   0   |
  | connectRetry    |   Bağlantı sağlanamaması durumunda aynı ip adresi için kaç bağlantı denemesi yapacağını belirtir. Tüm adresleri denedikten sonra hala bağlantı sağlanamaması durumunda yeniden baştan başlayarak bağlantı denemeleri devam eder.  |   3   |
  | connectRetryInterval    |   Her bağlantı denemesi sonrasında yeni denemenin kaç ms sonrasında başlayacağını belirtir.   |   300 |
  | receiveBufferSize   |   TCP yuvası için alım tampon bellek büyüklüğünü belirtir.    |   65536   |
  | sendBufferSize  |   TCP yuvası için gönderim tampon bellek büyüklüğünü belirtir.    |   65536   |

-   **ExecuteAsync(cancellationToken, param1, param2…) Metodu**

RedisClient nesnesi oluşturulduğunda istemci ile Redis sunucusu arasında
bir TCP/IP bağlantısı sağlanır. İstemciden sunucuya iletilen acil
mesajlar dışında tüm asenkron istekler bu metot üzerinden iletilir. İki
farklı kullanımı vardır. Parametreleri birinde string olarak
gönderilebilirken diğerinde byte\[\] olarak gönderilebilir. Gönderilen
her parametre komut segmentleri olarak sunucuya gönderilir.

**ExecuteAsync** metodu yapısı gereği istekleri sıraya sokar ve gönderim
sırasındaki paralel istekleri yoğun trafik altında tampon bölgeye
yazdıktan sonra topluca gönderim yapar. Bu veri transferi sırasında
yüksek performans elde edilmesini sağlar.

***NOT :** Kesinlikle **GetAwaiter().GetResult()** kullanılarak sonuç
elde edilmeye çalışılmamalıdır. Veri yazma işleminde paralel işlemler
optimize edildiğinden dolayı mümkün olduğunca paralel işlem başlatma
eğilimindedir. **GetAwaiter().GetResult()** ya da doğrudan **Result**
özelliği üzerinden sonuç elde edilmeye çalışıldığında cevabı aynı işlem
parçacığı **(thread)** üzerinde sonuçlandırmaya zorlandığı için aşırı
derece yavaşlama görülür. Her zaman Async/Await metodlar içinde
kullanlmadılır.*

Cevabı bir “Task” nesnesi olarak geriye döner. Redis sunucusu üzerinden
yanıt döndüğünde “Task” nesnesi tamamlandı durumuna geçer ve dönen değer
elde edilir.

Kullanım şekilleri aşağıdaki gibidir;

string value = await ExecuteAsync(cancellationToken, “GET”, “key”);

string value = await ExecuteAsync(cancellationToken, byteArray1,
byteArray2);

-   **Execute(cancellationToken, param1, param2…) Metodu**

ExecuteAsync metodu ile aynı işlevi sağlamaktadır. Ancak asenkron işlem
optimizasyonu içermemektedir.

Redis sunucusu üzerinden yanıt döndüğünde cevabı bir “string” nesnesi
olarak geriye döner.

Kullanım şekilleri aşağıdaki gibidir;

string value = Execute(cancellationToken, “GET”, “key”);

string value = Execute(cancellationToken, byteArray1, byteArray2);

-   **ExecuteEmergencyAsync(param1, param2…) Metodu**

ExecuteAsync metodu gibi sunucuya komut ve veri iletmekle sorumludur.
Fakat gönderilecek komutlar sıra beklemeden en ön sıraya alınması
gereken acil metotlardır. Bu yüzden gönderilecek mesajı performans
gözetmeden hemen en ön sırada sunucuya tek seferde iletilir.

Genellikle ilk bağlantı sonrasında **“db, password, clientCache”**
ayarları için ilgili komutları göndermek için kullanılır.

Bu metodun kullanılabilmesi için acil durum başlatıcısının kullanılması
gerekir. (**BeginEmergency()**)

Bu gönderimin yapılabilmesi için acil olmayan kanal durdurulur.
Flip-Flop yöntemi ile iki kanalın aynı anda devreye girmesi önlenir.

-   **GetAsync&lt;T&gt;(string key, CancellationToken cancellationToken
    = default) Metodu**

Verilen anahtara ait veriyi Redis üzerinden elde eder ve **T** tipinde
geri serileştirme yaparak üretilen objeyi geriye döner. Eğer istemci
taraflı önbellek özelliği aktif ise getirdiği değeri istemci tarafında
hafızaya alır. Tekrar anahtar talep edildiğinde önce istemci hafızasında
anahtarı kontrol ettikten sonra eğer yoksa Redis üzerinden anahtar
karşılığını sorgular.

İkinci parametrede verilen cancellationToken nesnesi redis işlemini
gerekli durumlarda iptal edecektir. Redis işlemi iptal olduğunda “Task
Canceled” istisnası fırlatılır.

Kullanım şekli aşağıdaki gibidir;

T value = await GetAsync&lt;T&gt;(“KEY”, cancellationToken);

-   **GetAsync(string key, CancellationToken cancellationToken
    = default) Metodu**

Verilen anahtara ait veriyi Redis üzerinden elde ederek **string**
tipinde geriye döner. Eğer istemci taraflı önbellek özelliği aktif ise
getirdiği değeri istemci tarafında hafızaya alır. Tekrar anahtar talep
edildiğinde önce istemci hafızasında anahtarı kontrol ettikten sonra
eğer yoksa Redis üzerinden anahtar karşılığını sorgular.

İkinci parametrede verilen cancellationToken nesnesi redis işlemini
gerekli durumlarda iptal edecektir. Redis işlemi iptal olduğunda “Task
Canceled” istisnası fırlatılır.

Kullanım şekli aşağıdaki gibidir;

string value = await GetAsync(“KEY”, cancellationToken);

-   **SetAsync&lt;T&gt;(string key, T data, TimeSpan? expiry,
    CancellationToken cancellationToken = default) Metodu**

Verilen anahtar ile veriyi Redis üzerine yazma işlemi için kullanılır.
Yazacağı **T** tipindeki veriyi serileştirme yaparak **JSON** string
olarak Redis üzerine yazar.

İkinci parametrede verilen cancellationToken nesnesi redis işlemini
gerekli durumlarda iptal edecektir. Redis işlemi iptal olduğunda “Task
Canceled” istisnası fırlatılır.

Kullanım şekli aşağıdaki gibidir;

bool success = await SetAsync&lt;T&gt;(“KEY”, data,
TimeSpan.FromSeconds(100), cancellationToken);

-   **SetAsync(string key, string data, TimeSpan? expiry,
    CancellationToken cancellationToken = default) Metodu**

Verilen anahtar ile veriyi Redis üzerine yazma işlemi için kullanılır.
Yazacağı **string** tipindeki veriyi Redis üzerine yazar.

İkinci parametrede verilen cancellationToken nesnesi redis işlemini
gerekli durumlarda iptal edecektir. Redis işlemi iptal olduğunda “Task
Canceled” istisnası fırlatılır.

Kullanım şekli aşağıdaki gibidir;

bool success = await SetAsync(“KEY”, data, TimeSpan.FromSeconds(100),
cancellationToken);

-   **ClearAsync(CancellationToken cancellationToken, params
    string\[\] keys) Metodu**

Verilen anahtarları Redis üzerinden temizler ve kaç adet temizlendiğini
geriye döndürür.

İlk parametrede verilen cancellationToken nesnesi redis işlemini gerekli
durumlarda iptal edecektir. Redis işlemi iptal olduğunda “Task Canceled”
istisnası fırlatılır.

Kullanım şekli aşağıdaki gibidir;

long deleteCount = await ClearAsync(cancellationToken, “KEY1”, “KEY2”,
“KEY3”);

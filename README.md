## The Cache Technology And High Performance .NET Redis Client With RESP3 Protocol Support

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

 I talked to our IT director Utku Tatl??dede about using the Redis
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


## ??nbellekleme(Cache) Teknolojileri ve RESP3 Protokol Destekli Y??ksek Performansl?? .Net Standart Redis Client

-   **Cache(??nbellek) Nedir?**

Yaz??l??m terimleri aras??nda ??ok s??k kulland??????m??z ???Cache??? yani ?????nbellek???
hayat??m??z??n her noktas??nda kulland??????m??z ve sistemlerin ??al????mas??nda
hayati ??nem ta????yan bir terimdir. Yaz??l??m sekt??r?? d??????nda da ??rneklerine
s??k rastlayabilece??imiz konulardand??r. ??rne??in bir yemek yap??m??nda ya da
bir tamirat i??leminde kullan??lmas?? planlanan malzemelerin tezg??h
??zerinde haz??r olarak bulundurulmas?? da bir ??nbellek olu??turulmas?? ile
ayn?? i??levi g??r??r. Burada temel nokta ??udur; her ne yapmak i??in haz??r
olarak bulunduraca????n??z her ne ise kullan??m noktas??na en yak??n yerde
bulundurulmal??d??r. Yani yemek yap??m?? i??in haz??rda bulundurman??z gereken
materyalleri yeme??i yapt??????n??z tezg??h yerine kilerde ya da dolapta
haz??rda bekletirseniz, size yemek yap??m?? s??ras??nda yeterli kolayl?????? ve
h??z?? kazand??rmayacakt??r.

Yaz??l??m meslek hayat??m??zda bu mekanizmalar?? o kadar yo??un kullan??yoruz
ki kulland??????m??z?? fark etmedi??imiz bir??ok nokta olmas?? muhtemeldir.
??rne??in s??radan bir metot i??inde kulland??????m??z de??i??kenler, ??al????ma
zaman??nda metot i??inde birden fazla kullan??ld?????? durumlar d??????n??lerek
RAM ??zerine s??rekli ba??vurmamak i??in i??lemcinin belle??ine al??n??rlar (L3
b??lgesi). RAM eri??im h??z?? bir http ??a??r??s??na k??yasla bizim hi?? k??lfetini
dikkate almayaca????m??z kadar h??zl?? olsa bile i??lemcinin i??lem yapmak i??in
ihtiyac?? olan her ne ise malzemelerini en yak??n b??lgesinde haz??rda
bekletme e??iliminde tasarland??????n?? g??r??rs??n??z.

Ancak baz?? durumlarda ??oklu i??lemlerin payla??t?????? ortak veriler s??z
konusu oldu??unda en g??ncel verinin i??lenmesi gerekebilir. Bu durumda her
ne kadar h??z kayb?? ya??anacak olsa da i?? kural?? gere??i ??nbellek
mekanizmas?? devre d?????? b??rak??l??r. ????lemci ??rne??inde de??i??kenlerin
??nbellek kullan??mlar??n?? engellemek i??in C\# dilinde de??i??kenler
tan??mlarken **???volatile???** anahtar kelimesinin kullan??lmas?? yeterlidir.
Bu sayede her de??i??ken eri??iminde elde edilen veri RAM ??zerinden
getirilir.

-   **??nternet ??zerinden Kullan??lan D???? Servis Verileri ve ??nbellekleme
    ????lemi**

Yaz??l??mc??lar olarak geli??tirdi??imiz ??o??u sistem internet ??zerinden
birbirine ba??lanmaktad??r. ???? kurallar??m??z gere??i internet ??zerindeki
bir??ok kaynaktan elde edilen veriler ??zerinden i??lemlerimizi y??r??t??r??z.
E??er bu veriler i??inde kayna????nda ??ok s??k de??i??meyenler varsa veya
de??i??ti??i zaman anl??k olarak g??ncel verinin kullan??lmas?? zorunlu
de??ilse, bu verileri ilk servis ??a??r??s?? sonras?? RAM ??zerine yazmak ve
bir sonraki ihtiya??ta RAM ??zerinden kullanmak, veriyi tezgah??m??za
yakla??t??rmak olacakt??r ve yeniden eri??imimizde bize y??ksek performans
kazan??m?? sa??layacakt??r.

Bu noktaya kadar bahsetti??imiz ??nbellekleme i??lemleri temel ve
halihaz??rda ??ok ??aba sarf etmeden kulland??????m??z teknolojilerdir. Bu
noktadan sonraki ihtiya??, ayn?? ortak veriye internet ??zerindeki farkl??
uygulamalardan eri??meye ??al????t??????m??z noktada biraz daha karma????kla??maya
ba??l??yor. ????nk?? burada sistemimizde bir grup uygulama var ve hepsinin
ayn?? veri ??zerinde ??al????mas?? gerekiyor. Bu da ortak bir noktadan e??it
g??ncel veri ile ??al????malar??n?? garanti etmek demektir ve bu durumda her
uygulama do??rudan kendi lokal de??i??kenlerinde bu veriyi saklayamaz. E??er
saklarlarsa di??er uygulamalarla birlikte ayn?? veriyi kulland??????n??
garanti edemezler.

Burada iki y??ntemle ilerlenebilir;

-   Birincisi, en g??ncel veriye kayna????ndan eri??mek,

-   ??kincisi ise veriyi kayna????ndan al??p, uygulamalara kayna????ndan daha
    yak??n ortak ba??ka bir noktaya koyarak eri??mek.

E??er veri herhangi bir i??lemden ge??miyor ve veri kayna????n??n yak??nl??????
uygulamalar i??in kullan??labilir herhangi bir ortak nokta kadar yak??nsa
do??rudan kayna????ndan eri??mek mant??kl?? olabilir. Fakat okuma yazma h??z??
verinin kayna????nda i??lem gerektiriyor ve yak??nl??k anlam??nda da yeterince
yak??n de??ilse, veriyi konumland??rmam??z gereken yak??n bir nokta ve bu
noktan??n da eri??imde h??zl?? teknolojiler i??ermesi gerekiyor. Bu tip
sistemlere **???Da????t??k ??nbellek Sistemleri(Distributed Cache Systems)???**
ad?? veriliyor. ??rnek olarak Redis ya da Memcached uygulamalar?? bu tip
da????t??k ??nbellekleme sistemleri sunan ??r??nlerden baz??lar??d??r. Veri
saklama elde etme ve transfer protokollerinde y??ksek performans
hedeflenerek tasarlanm????lard??r.

-   **??l??eklenebilir Sistemlerde ??nbellekleme ????lemi**

G??n??m??zde internetin ??neminin artmas?? ile olu??an yo??un trafi??i
kar????layabilmek ad??na, sunulan bir uygulaman??n gerekti??inde ??artlara
g??re say??s??n??n art??r??labilmesi ya da i??inde bulundu??u sunucunun
kaynaklar??n??n art??r??labilmesi gerekmektedir.

-   Sunucu kaynaklar??n??n art??r??lmas?? i??lemine **???Dikey ????eklendirme
    (Vertical Scaling)???** ,

-   Ayn?? sunucu kaynaklar?? ??zerinde uygulama say??s??n??n art??r??lmas?? ise
    **???Yatay ??l??eklendirme (Horizontal Scaling)???** ad?? veriliyor.

Yatayda ??l??eklendirme ihtiyac??, uygulama say??lar??n?? art??r??r ve
??nbellekleme i??lemlerinin birden fazla uygulamada y??netilmesi gerekir.
Bu durumda veri tutarl??l??????n??n ??nemli olmas??ndan dolay?? ??nbellek
i??lemlerinin da????t??k ??nbellekleme sistemleri ??zerinden yap??lmas??
gerekebilir.

Da????t??k ??nbellekleme sistemleri hi??bir zaman i??lemlerin
ger??ekle??tirildi??i noktaya en yak??n y??ntem de??ildir. Bu y??zden bir
noktada performans??n mutlaka y??netilmesi gerekir. ??rne??in Redis sunucusu
??zerinde tutulan bir veriye ula??mak veya o veriyi de??i??tirmek a??
transferi ??zerinden sa??land?????? i??in bir d??ng??de binlerce kez bir veriye
eri??im ihtiyac?? duyuldu??unda a?? bant geni??li??ini ciddi oranda
t??ketecektir. Ancak di??er y??nden de veriyi do??rudan her uygulama i??in
i??lem b??lgesine en yak??n yerlerden biri olan RAM b??lgesine yazm????
olsayd??k her bir uygulama i??in verinin g??ncelli??ini de garanti alt??na
almam??z gerekirdi.

Bu problemler ??????????nda yatay ??l??eklenebilir sistemlerde baz?? **da????t??k
yak??n haf??za ??nbellekleme (Distributed InMemory Cache)** ????z??mleri
??retilmi?? ve kullan??lmaktad??r. En yayg??n ????z??mde eri??ilmek istenen veri,
uygulamalar??n yak??n haf??zas?? (RAM) ve ortak bir noktadaki da????t??k
??nbellek (Distributed Cache) noktalar??nda birlikte bar??nd??r??lmaktad??r.
Uygulamalar yak??n haf??zalar??ndaki verilerin g??ncelli??inin de??i??ti??i
durumlar?? ????renmek i??in ayr??ca bir mesaj kanal?? dinlerler. ??lgili
verileri da????t??k ??nbellek ??zerinde g??ncelleyen ba??ka uygulamalar ise bu
mesaj kanal??na de??i??im mesajlar??n?? b??rak??rlar. De??i??im mesajlar??n?? alan
uygulamalar yak??n haf??zalar??ndaki de??i??en verileri silerler ve ihtiya??
duyduklar??nda tekrardan tek seferlik olarak ortak noktadaki da????t??k
??nbellek ??zerinden elde ederek tekrar kullanmak ??zere yak??n haf??zalar??na
kaydederler. B??ylelikle uygulamalar s??rekli a?? ??zerindeki ortak noktaya
ba??vurarak bant geni??li??i t??ketmek yerine, en son elde edildikten sonra
de??i??im mesaj?? gelmemi?? yak??n haf??zalar??na ald??klar?? veriyi kullanarak
veriye daha h??zl?? bir y??ntemle ula??m???? olurlar. De??i??im mesajlar?? ile de
verinin g??ncelli??ini sa??lam???? olurlar.

Bu y??ntem bir problemi ????zmeye ??al??????rken uygulad?????? mekanizma ile baz??
problemleri de ortaya ????kart??r. Bunlardan baz??lar?? ??unlard??r:

-   Da????t??k nokta ??zerinde ??nbelle??e al??nan veriler genellikle bir
    ge??erlilik s??resi ile kay??t alt??na al??n??rlar (Time to Live.
    Bkz: TTL). Yak??n haf??zaya konumland??r??lan veri elde edilirken bu
    s??re g??zetilmeden kay??t alt??na al??n??r. E??er bu ge??erlilik s??resi
    i??inde veriye ait bir de??i??im mesaj?? g??nderilmemi??se veri da????t??k
    ??nbellek noktas??nda silinmi?? olabilir. Bu durumda uygulama yak??n
    haf??zas??nda bulunan verinin g??ncelli??ini garanti edemez.

-   Bu sistem, yak??n haf??zaya al??nm???? verinin g??ncelli??inin bir mesaj
    kanal?? ??zerinden kontrol edilmesi ??zerine kurulmu??tur. Ortak
    noktadaki da????t??k ??nbellek ??zerinde bu sistemle ??al????mayan bir
    uygulama taraf??ndan yap??lan ya da insan eliyle yap??lacak bir
    de??i??iklik, bu mesaj kanal?? vas??tas?? ile de??i??im
    mesajlar?? iletmeyecektir. Bu durumda uygulama yak??n haf??zas??nda
    bulunan verinin g??ncelli??ini garanti edemez.

-   **LCWaikiki Teknoloji D??n??????m Projelerinde ??nbellekleme**

LCWaikiki uygulamalar??nda yatay ??l??ekleme yapt??????m??z yo??un noktalarda
??nbellekleme i??in Redis uygulamas??n?? tercih ediyoruz. Birka?? ay ??ncesine
kadar baz?? noktalarda yukar??da bahsetti??im da????t??k yak??n haf??za
??nbellekleme tekni??ini kullan??yorduk.

Haziran 2021 itibari ile LCWaikiki E-Ticaret ailesinde kullan??lan
LocalizationService uygulamas??n?? teknoloji d??n??????m??ne dahil ettik. Redis
ba??lant??s?? i??in modernizasyonda kullanmak ??zere daha ??nceki projelerde
de kulland??????m??z **Stackexchange.Redis** k??t??phanesini tercih etmi??tik.

Teknoloji d??n??????m projelerimiz devam ederken bireysel olarak baz??
transfer teknolojileri ??zerinde de ara??t??rma ve ??al????malar??m vard?? ve bu
teknolojileri s??f??rdan geli??tirdi??im bir Redis Client uygulamas??
??zerinde uygulayarak denemeler yap??yordum. Bu ??al????ma ile Windows
tabanl?? Redis Desktop Manager uygulamas?? benzeri Web tabanl?? Redis Web
Manager uygulamas?? geli??tirmeye ??al??????yordum.

D??n??????m mimarisini tasarlarken ki??isel ara??t??rmalar??mdan esinlenerek
RESP3 protokol?? ile gelen ge??ersizlik mekanizmas??n?? (**Invalidate
Mechanism**) kullanmay?? planlad??m. Da????t??k yak??n haf??za
??nbellekleme(**Distributed InMemory Cache**) sistemi i??in gerekli olan
bu mekanizma, Redis sunucusu versiyonlar??ndan 6.0.0 versiyonu ve
sonras??nda desteklenmektedir.

Bu sistemi tercih etme sebebim yukar??da bahsetti??im problemlerden
ka????nmak ve ayn?? zamanda a??a????daki faydalar?? sa??layabilmekti:

-   RESP3 protokol??, RESP2 de ayr??ca ba??ka bir kanal gerektiren pub-sub
    mekanizmas??n?? tek kanaldan y??netebiliyor. Bu da ba??lant?? say??lar??n??
    yar?? yar??ya d??????r??yor.

-   RESP3 protokol??nde istemcileri takip etme mekanizmas??
    **(client tracking)** hangi istemcinin en son hangi anahtar kelimeyi
    sorgulad??????n?? tutuyor. Hangi ??ekilde de??i??iklik ger??ekle??irse
    ger??ekle??sin, de??i??en ya da TTL s??resi dolan anahtar kelimeyi sadece
    hangi istemciler kullanm????sa onlara protokolde ge??en ???PUSH??? nesnesi
    olarak ???ge??ersiz??? mesaj?? iletiyor.

    -   Mesaj trafi??i ??ssel derecede azal??yor.

    -   Di??er y??ntemlerde pub-sub mesajlar?? uygulama seviyesinde
        y??netilirken, bu y??ntemde veri kayna???? olan sunucu her ne
        ??ekilde olsun de??i??imin ilgili istemcilere iletilmesini
        garanti ediyor.

    -   RESP3 d??????ndaki uygulama seviyesinde y??netilen ge??ersizlik
        mesajlar?? TTL durum de??i??ikliklerini iletmeyi garanti edemez.
        Ayn?? zamanda uygulama seviyesinde y??netilen ge??ersizlik
        mesajlar?? i??eren sistemlerde y??netimi yapmayan uygulamalar??n
        de??i??tirdi??i anahtarlar??n ge??ersizlik mesajlar?? di??er
        uygulamalara iletilemeyecektir. Bu da veri tutarl??l??????n?? bozar.

**Stackexchange.Redis** k??t??phanesi Redis sunucusunun iletimde
kulland?????? RESP protokol versiyonlar??ndan RESP1 ve RESP2 yi
destekliyorken RESP3 protokol??n?? desteklememektedir. Pandemi ve
teknolojik geli??melerin destekledi??i y??ksek veri trafi??ine ek olarak bir
de d??nemsel kampanyalar??n getirece??i y??ksek veri trafi??ini de ele
ald??????m??zda RESP3 teknolojisini mutlaka kullanmam??z gerekti??ini
d??????nd??m.

Halihaz??rda bireysel olarak denemelerimde kulland??????m Redis ba??lant??
sa??lay??c??s?? mekanizmas??n?? ??zelle??tirerek paket haline getirip d??n??????m
projemizde kullanmak ??zere direkt??r??m??z Utku Tatl??dede ile g??r????t??m.
Kendisi de avantajlar??n?? dikkate de??er buldu ve d??n??????m projesinde
deneme konusuna s??cak bakt??.

Kas??m kampanyalar??nda Stackexchange.Redis kullanan yo??un yerler cevap
vermekte zorlan??nca o noktalar?? RESP3 protokol destekli yeni
geli??tirdi??im Redis ba??lant?? sa??lay??c??s?? ile de??i??tirdik. Ve kas??m
kampanyas??n?? olduk??a rahat bir ??ekilde ge??irdik.

Sonu?? olarak evrensel bir k??t??phane geli??tirmi?? oldum ve LCWakiki
y??netimimizin deste??i ile de ortaya ????kan ??r??n??n yeteneklerini ispat
etmi?? olduk. Kodlar??na a??a????daki github adresinden eri??ebilirsiniz.
Sizlerin de deste??i ile k??t??phanenin yeteneklerini daha da ileriye
ta????yarak teknolojiye el birli??i ile katk??da bulunmu?? olaca????z.

[*https://github.com/orgs/TheUniversalCity/repositories*](https://github.com/orgs/TheUniversalCity/repositories)

-   **RESP3 Redis Ba??lant?? Sa??lay??c??s?? Yap??s??**

Redis veri transferini TCP/IP transfer protokol?? ??zerinden RESP uygulama
katman?? protokol?? ile yapmaktad??r. Redis???in veri transferi protokol??
olan RESP yap??s??n?? anlayabilmek i??in TCP/IP protokol?? hakk??nda temel
bilgilerin g??zden ge??irilmesi gerekmektedir.

TCP/IP, iki cihaz aras??nda ??ift y??nl?? ???byte??? dizisi transferi yapan
iletim protokol??d??r. Bir TCP ba??lant??s?? sa??land??????nda fiziksel olarak
??ift y??nl?? **(Full-Duplex)** bir ba??lant?? kanal?? a????l??r. Bu ba??lant??
??zerinde birbirinden ba????ms??z iki y??nde de ayn?? anda veri iletimi
yap??labilir. Bu veri iletimi ile iki taraf birbiri ile haberle??mi?? olur.

![](duplex.jpg)

TCP tan??m??nda taraflar ???Aktif??? ve ???Pasif??? olarak adland??r??l??rlar.

**Pasif Taraf:**

Bir ba??lant??n??n sa??lanabilmesi i??in her zaman bir taraf??n ba??lant??y??
??nceden a??mas?? gerekir. Bu g??revi pasif taraf ??stlenir. Kar????s??nda bir
taraf beklemeden bir kap?? **(Port)** a??abilen pasif taraf, kap??y??
a??t??ktan sonra ba??lant?? taleplerini kabul etmeyi bekler. Bir pasif
taraf, a??m???? oldu??u kap??dan fiziksel olarak ayn?? anda birden fazla aktif
taraf ile el s??k????abilir. Dinleyici rol?? sebebiyle kendisine ba??lant??
iste??i atan aktif taraf??n g??revlerini bilmek zorunda de??ildir.

-   Aktif taraftan ald?????? komut ve verilerle bir g??rev yerine getirerek
    yan??t d??ner.

-   ??stlendi??i g??revlere ili??kin herhangi bir komut ya da veri
    beklemeden do??rudan el s??k????t?????? aktif tarafa veri
    iletimi sa??layabilir.

Bu ??zellikleri nedeni ile **???sunucu (Server)???** olarak bilinir. Redis
sunucusu kurdu??u ba??lant??larda pasif taraft??r.

**Aktif Taraf:**

Aktif taraf her zaman konumunu bildi??i bir pasif taraf ile el s??k????arak
ba??lant??y?? sa??lamak zorundad??r. Bir aktif taraf sadece bir pasif taraf
ile fiziksel olarak el s??k????abilir. ??stemci rol?? nedeni ile ba??lant??
iste??i att?????? pasif taraf??n ??stlendi??i g??revleri bilir ve isteklerinin
kar????lanmas??n?? bekler. Bu nedenle **???istemci (Client)???** ad?? ile
bilinir. Redis sunucusuna ba??lant?? kuran istemciler kurduklar??
ba??lant??da aktif taraft??rlar.

Bu aktif ve pasif taraflar TCP ba??lant??s?? ??zerinden iletilen verileri
anlamland??rmak ve kar????l??kl?? haberle??ebilmek i??in kendi aralar??nda bir
dil geli??tirmeleri gerekir. TCP gibi ??ift tarafl?? (Duplex) transfer
protokollerinde fiziksel olarak ayn?? anda veri g??nderilebilir ve
al??nabilir. B??ylelikle de istemci bir talepte bulunaca???? veriyi
iletti??inde sunucunun tekrar o iste??in cevab??n?? ne zaman d??nece??i belli
de??ildir. Ve hangi cevab??n hangi iste??in cevab?? oldu??unu bilmek gerekir.
Ayr??ca al??nan verinin ne kadar??n??n bir cevap oldu??unu da bilmek gerekir.
T??m bu karma??an??n nas??l i??leyece??ini belirleyen ve istekler ile
cevaplar??n belirli bir d??zende y??netilmesini sa??layan standart
kurallar??n tamam?? uygulama seviyesinde bir ??st protokol olu??turur.
**RESP (REdis Serialization Protocol)** de bu protokoller aras??ndad??r.

-   **RESP (REdis Serialization Protocol)**

Redis bir sunucudur ve kendisine ba??lanan istemcilerin ihtiya??lar??n?? bir
protokol??n kurallar?? ??er??evesinde kar????lar. Bu protokol tan??m??na
**???REdis Serialization Protocol???** denilmektedir. ???RESP??? bu ifadenin
k??salt??lm???? halidir.

RESP en temelde Redis sunucusuna TCP ile ba??lanan istemcilere benzersiz
bir numara atanmas??n?? garanti eder. Bu ??ekilde istemcilerle ileti??im
kurarken atanm???? bir numaras?? bulunan istemcilerden gelen komutlar??n
cevaplar??n??n ayn?? s??rada ayn?? numaradaki istemciye iletilmesini garanti
etmektedir. Bu tekni??e ileti??im teknolojilerinde s??kl??kla kullan??lan
**?????o??ullama (Multiplexing)???** denilmektedir.

![](multiplexing.jpg)

Fiziksel olarak tek kanaldan yap??lan veri transferlerinde ayn?? anda bir
y??nde yaln??zca bir veri transferi yap??labilir. Bir ??ok farkl?? mesaj??n
birle??tirildikten sonra transfer edilip arkada ayr????t??r??lmas?? i??lemine
**?????o??ullama (Multiplexing)???** ad?? verilmektedir. Redis ileti??iminde
??o??ullama i??lemini RESP kurallar?? sa??lar. Redis iletiminde veriler
serile??tirme kurallar??na g??re b??t??n halinde ve s??ral?? bir ??ekilde
iletilir. Cevaplar??n da ayn?? s??ra ile iletilece??i garanti edilir.

Redis ??o??ullama yap??s??nda s??ra y??nteminin kullan??lmas??ndaki tek etmen
y??ksek h??zd??r. Di??er baz?? protokollerde mesajlara ait numaraland??rma
y??ntemleri kullan??larak bir mesaj??n par??alar?? mesaja ait numara ??st
bilgisi ile kar??????k halde de iletilebilmektedir. Ancak bu y??ntem,
mesaj??n par??alanmas?? ve birle??tirilmesi s??ras??nda ??st bilgilerin
yorumlanmas?? gibi ek i??lemler gerektirmektedir. Bu da performans kayb??na
sebep olmaktad??r. Redis performans kazanmak i??in bu ??st bilgileri
kullanmadan veriyi b??t??nler halinde s??ra ile g??nderir ve g??nderdi??i
s??ran??n ald?????? komut s??ras??nda oldu??unu garanti eder.

HTTP gibi baz?? ??st protokollerde iletilen verinin yorumlanmas?? veya
cevapla ili??kilendirmeyi kolayla??t??racak baz?? ??stbilgi(metadata)
tan??mlar?? bulunur. ??rne??in HTTP veri transferlerinde iletilen verinin
yap??s??n?? sunucunun tan??yabilmesi i??in ???Content-Type??? ba??l?????? ile
i??eri??in tipi de ??st bilgi olarak eklenir. RESP iletimde y??ksek h??z??
hedefledi??inden iletim s??ras??nda veri d??????nda hi??bir
??stbilgiyi(metadata) bar??nd??rmaz.

A??a????daki redis cevab??n?? inceleyebilirsiniz:

\$11&lt;CR&gt;&lt;LF&gt;Gelen Cevap&lt;CR&gt;&lt;LF&gt;

Bu mesaj istemci taraf??na cevap olarak iletilirken ba??tan ba??layarak
s??ra ile iletilir. ??stemcinin bu cevab?? ????zebilmesi i??in verinin tipini
ve transferin nerede son bulaca????n?? bilmesi gerekir. Bu y??zden RESP
tan??m??nda ilk ??nce verinin tipi iletilir. Yukar??daki ???\$??? harfi bu
verinin ???Bulk String??? oldu??unu belirtir. ???\$??? ile ???&lt;CR&gt;&lt;LF&gt;???
aras??nda g??nderilen t??m rakamlar verinin boyutunun ka?? bayt olaca????n??
belirtir. Yukar??da g??r??nen 11 say??s?? ilk ???&lt;CR&gt;&lt;LF&gt;??? ile son
???&lt;CR&gt;&lt;LF&gt;??? aras??nda 11 bayt okumas?? gerekti??ini bildirir.
Detayl?? bilgiye a??a????daki ba??lant?? ??zerinden ula??abilirsiniz.

[*https://redis.io/topics/protocol*](https://redis.io/topics/protocol)

Bu ??nden bilgilendirme yap??s?? ile veri transfer edilirken ayn?? zamanda
yorumlama i??lemi de yap??labildi??inden olduk??a verimli bir y??ntemdir.

Redis komut i??leme ve cevap g??ndermenin yan??nda bir mesaj kanal??na
abonelik i??lemini (PUB/SUB) de destekler. RESP1 ve RESP2 tan??mlar??nda
bir istemci sunucuya belli bir mesaj kanal?? i??in abone oldu??unda komut
g??ndermeksizin sunucu ??zerinden komut bekler. Bu nedenle s??ral??
??o??ullama ??zelli??ini kaybedece??i i??in a????lan kanal??n protokol?? PUSH
protokol??ne d??n????m???? olur. Dolay??s?? ile komut g??nderme ve cevap alma
yetene??ini kaybeder.

RESP3 tan??m??nda ise ??o??ullama yetene??i eklenen yeni nesne t??rleri ile
geni??letilmi??tir. Sunucunun g??nderdi??i otomatik mesajlar?? istemciye yeni
tan??mla gelen PUSH nesnesi ile g??nderildi??i i??in g??nderilen s??ral??
komutlar i??in beklenen di??er objeler ay??rt edilir ve s??ras??
kar????t??r??lmadan ileti??im devam eder. Bu sayede ayn?? istemci kanal??
??zerinden komut g??nderilip cevaplar?? al??n??rken ayn?? zamanda sunucu
taraf??ndan g??nderilen abonelik mesajlar?? da istemci taraf??ndan komut
cevaplar?? ile kar????t??r??lmadan elde edilmi?? olur.

RESP3 tan??m?? PUSH bildirimlerini de bar??nd??rd??????ndan dolay?? istemci
taraf?? ??nbellekleme (Client Cache) i??lemlerini de destekler haline
gelmi??tir. ????nk?? RESP3 tan??m??nda geni??letilmi?? PUSH bildirimleri
sayesinde istemciler, anahtara kar????l??k gelen de??erleri sorgulad??????
kanaldan ayn?? zamanda daha ??nce sorgulam???? oldu??u anahtarlardan ge??ersiz
hale gelenlerin bildirimlerini de alabilme yetene??i kazanm????t??r.

A??a????daki komut sunucuya iletildi??inde;

**CLIENT TRACKING on**

Redis ilgili istemciyi **ge??ersizlik tablosuna (INVALIDATE TABLE)**
kaydeder. Bu tablo hangi anahtar??n hangi istemci taraf??ndan
sorguland??????n?? kay??t alt??nda tutar. E??er bu tabloda bulunan
anahtarlardan birisi ge??ersiz hale gelirse, yani silinir, g??ncellenir ya
da ya??am s??resi son bulursa, ge??ersizlik tablosu ??zerinde hangi
istemcinin bu anahtar?? sorgulad??????n?? tespit edip bu istemcilere
a??a????daki gibi bir **???Ge??ersizlik Bildirimi (Invalidate Push)???**
g??nderir.

&gt;4&lt;CR&gt;&lt;LF&gt;

+invalidate&lt;CR&gt;&lt;LF&gt;

+key1&lt;CR&gt;&lt;LF&gt;

+key2&lt;CR&gt;&lt;LF&gt;

+key3&lt;CR&gt;&lt;LF&gt;

Bu bildirimi alan istemci, kendi taraf??ndaki ??nbellekte saklad?????? key1,
key2 ve key3 e ait verileri siler ve yeniden bu anahtarlar i??in bir
talep gelirse bu talebi ??nbelle??inden de??il Redis ??zerinden kar????lar.

E??er bir istemcinin ba??lant??s?? sonlan??rsa ilgili istemciye ait Redis
??zerindeki ge??ersizlik tablosundaki veriler de silinir. Bunun ya??anmas??
durumunda istemci kendi belle??ine ald?????? t??m anahtarlar?? silmelidir.
Art??k yeni kuraca???? ba??lant??da Redis sunucusu taraf??ndan kendisine yeni
bir numara verilece??i i??in ??nceki ba??lant??da talep etti??i anahtarlara
ait ge??ersizlik mesajlar??n?? alamayacakt??r.

Bu protokol istemci ve sunucu aras??nda e??zamanl?? y??r??t??lmektedir. Bu
yetenekleri sisteme kazand??ran bir istemci k??t??phanesi kullanarak RESP3
protokol??n?? deneyebilirsiniz.

-   **TheUniversalCity.RedisClient**

[*https://github.com/TheUniversalCity/RedisClient*](https://github.com/TheUniversalCity/RedisClient)

.Net Standart Library ??zerinde geli??tirilmi?? olan
**TheUniversalCity.RedisClient** a??a????daki uygulama t??rlerinde
kullan??labilir.

  | **.NET uygulama**               |   **S??r??m deste??i**  |
  | ----------------------------    |   ------------------------------------------  |
  | .NET ve .NET Core               |   2.0, 2.1, 2.2, 3.0, 3.1, 5.0, 6.0   |
  | .NET Framework                  |   4.6.1, 4.6.2, 4.7.0, 4.7.1, 4.7.2, 4.8.0    |
  | Mono                            |   5.4, 6.4    |
  | Xamarin.iOS                     |   10.14, 12.16    |
  | Xamarin.Mac                     |   3.8, 5.16   |
  | Xamarin.Android                 |   8.0, 10.0   |
  | Evrensel Windows Platformu      |   10.0.16299, TBD |
  | Unity                           |   2018,1  |

-   **??stemci Yaratma ve Ba??lant??**

**RedisClient** s??n??f?? **static** bir **CreateClientAsync** metodu
i??erir. Bu metod kullan??larak yeni bir RedisClient nesnesi yarat??l??r
ve Redis ile ba??lant?? kurulur. Bu metodun iki farkl?? a????r??
y??klemesi(**override**) bulunmaktad??r.

var client = RedisClient.CreateClientAsync(\_connectionString)

ve

var client = RedisClient.CreateClientAsync(new RedisConfiguration())

**???\_connectionString???** string bir ifadedir. Ba??lant?? i??in gerekli
ayarlar??n verildi??i ba??lant?? c??mleci??idir. A??a????daki yap??da
kullan??lmaktad??r. En az bir IP adresi ya da host de??eri zorunludur.

**???\[ipAddress|host\]\[:\[port\]\],\[ipAddress2|host2\]\[:\[port\]\],\[optionKey\]=\[value\],\[optionKey2\]=\[value\]???**

??rnek: ???localhost,clientCache=true???

-   **Ayarlar (Options)**

Ba??lant?? c??mleci??i i??erisinde kullan??labilecek olan ayarlar ve
a????klamalar?? a??a????dad??r.

  | **Ayar (Option)**   |   **A????klama**    |   **Varsay??lan**  |
  | -----------------   |   ------------    |   --------------  |
  | clientCache |   ??stemci tarafl?? ??nbellek ??zelli??ini aktif eder. ???true??? ve ???false??? de??erleri al??r. ???true??? de??eri verebilmek i??in Redis versiyonunun 6.0.0 ve ??zerinde olmas?? gereklidir. |   false   |
  | password    |   Redis ba??lant??s?? i??in e??er varsa ??ifre de??erini al??r    |   null    |
  | db  |   Redis ba??lant??s?? i??in veritaban?? numaras??n?? belirtir.   |   0   |
  | connectRetry    |   Ba??lant?? sa??lanamamas?? durumunda ayn?? ip adresi i??in ka?? ba??lant?? denemesi yapaca????n?? belirtir. T??m adresleri denedikten sonra hala ba??lant?? sa??lanamamas?? durumunda yeniden ba??tan ba??layarak ba??lant?? denemeleri devam eder.  |   3   |
  | connectRetryInterval    |   Her ba??lant?? denemesi sonras??nda yeni denemenin ka?? ms sonras??nda ba??layaca????n?? belirtir.   |   300 |
  | receiveBufferSize   |   TCP yuvas?? i??in al??m tampon bellek b??y??kl??????n?? belirtir.    |   65536   |
  | sendBufferSize  |   TCP yuvas?? i??in g??nderim tampon bellek b??y??kl??????n?? belirtir.    |   65536   |

-   **ExecuteAsync(cancellationToken, param1, param2???) Metodu**

RedisClient nesnesi olu??turuldu??unda istemci ile Redis sunucusu aras??nda
bir TCP/IP ba??lant??s?? sa??lan??r. ??stemciden sunucuya iletilen acil
mesajlar d??????nda t??m asenkron istekler bu metot ??zerinden iletilir. ??ki
farkl?? kullan??m?? vard??r. Parametreleri birinde string olarak
g??nderilebilirken di??erinde byte\[\] olarak g??nderilebilir. G??nderilen
her parametre komut segmentleri olarak sunucuya g??nderilir.

**ExecuteAsync** metodu yap??s?? gere??i istekleri s??raya sokar ve g??nderim
s??ras??ndaki paralel istekleri yo??un trafik alt??nda tampon b??lgeye
yazd??ktan sonra topluca g??nderim yapar. Bu veri transferi s??ras??nda
y??ksek performans elde edilmesini sa??lar.

***NOT :** Kesinlikle **GetAwaiter().GetResult()** kullan??larak sonu??
elde edilmeye ??al??????lmamal??d??r. Veri yazma i??leminde paralel i??lemler
optimize edildi??inden dolay?? m??mk??n oldu??unca paralel i??lem ba??latma
e??ilimindedir. **GetAwaiter().GetResult()** ya da do??rudan **Result**
??zelli??i ??zerinden sonu?? elde edilmeye ??al??????ld??????nda cevab?? ayn?? i??lem
par??ac?????? **(thread)** ??zerinde sonu??land??rmaya zorland?????? i??in a????r??
derece yava??lama g??r??l??r. Her zaman Async/Await metodlar i??inde
kullanlmad??l??r.*

Cevab?? bir ???Task??? nesnesi olarak geriye d??ner. Redis sunucusu ??zerinden
yan??t d??nd??????nde ???Task??? nesnesi tamamland?? durumuna ge??er ve d??nen de??er
elde edilir.

Kullan??m ??ekilleri a??a????daki gibidir;

string value = await ExecuteAsync(cancellationToken, ???GET???, ???key???);

string value = await ExecuteAsync(cancellationToken, byteArray1,
byteArray2);

-   **Execute(cancellationToken, param1, param2???) Metodu**

ExecuteAsync metodu ile ayn?? i??levi sa??lamaktad??r. Ancak asenkron i??lem
optimizasyonu i??ermemektedir.

Redis sunucusu ??zerinden yan??t d??nd??????nde cevab?? bir ???string??? nesnesi
olarak geriye d??ner.

Kullan??m ??ekilleri a??a????daki gibidir;

string value = Execute(cancellationToken, ???GET???, ???key???);

string value = Execute(cancellationToken, byteArray1, byteArray2);

-   **ExecuteEmergencyAsync(param1, param2???) Metodu**

ExecuteAsync metodu gibi sunucuya komut ve veri iletmekle sorumludur.
Fakat g??nderilecek komutlar s??ra beklemeden en ??n s??raya al??nmas??
gereken acil metotlard??r. Bu y??zden g??nderilecek mesaj?? performans
g??zetmeden hemen en ??n s??rada sunucuya tek seferde iletilir.

Genellikle ilk ba??lant?? sonras??nda **???db, password, clientCache???**
ayarlar?? i??in ilgili komutlar?? g??ndermek i??in kullan??l??r.

Bu metodun kullan??labilmesi i??in acil durum ba??lat??c??s??n??n kullan??lmas??
gerekir. (**BeginEmergency()**)

Bu g??nderimin yap??labilmesi i??in acil olmayan kanal durdurulur.
Flip-Flop y??ntemi ile iki kanal??n ayn?? anda devreye girmesi ??nlenir.

-   **GetAsync&lt;T&gt;(string key, CancellationToken cancellationToken
    = default) Metodu**

Verilen anahtara ait veriyi Redis ??zerinden elde eder ve **T** tipinde
geri serile??tirme yaparak ??retilen objeyi geriye d??ner. E??er istemci
tarafl?? ??nbellek ??zelli??i aktif ise getirdi??i de??eri istemci taraf??nda
haf??zaya al??r. Tekrar anahtar talep edildi??inde ??nce istemci haf??zas??nda
anahtar?? kontrol ettikten sonra e??er yoksa Redis ??zerinden anahtar
kar????l??????n?? sorgular.

??kinci parametrede verilen cancellationToken nesnesi redis i??lemini
gerekli durumlarda iptal edecektir. Redis i??lemi iptal oldu??unda ???Task
Canceled??? istisnas?? f??rlat??l??r.

Kullan??m ??ekli a??a????daki gibidir;

T value = await GetAsync&lt;T&gt;(???KEY???, cancellationToken);

-   **GetAsync(string key, CancellationToken cancellationToken
    = default) Metodu**

Verilen anahtara ait veriyi Redis ??zerinden elde ederek **string**
tipinde geriye d??ner. E??er istemci tarafl?? ??nbellek ??zelli??i aktif ise
getirdi??i de??eri istemci taraf??nda haf??zaya al??r. Tekrar anahtar talep
edildi??inde ??nce istemci haf??zas??nda anahtar?? kontrol ettikten sonra
e??er yoksa Redis ??zerinden anahtar kar????l??????n?? sorgular.

??kinci parametrede verilen cancellationToken nesnesi redis i??lemini
gerekli durumlarda iptal edecektir. Redis i??lemi iptal oldu??unda ???Task
Canceled??? istisnas?? f??rlat??l??r.

Kullan??m ??ekli a??a????daki gibidir;

string value = await GetAsync(???KEY???, cancellationToken);

-   **SetAsync&lt;T&gt;(string key, T data, TimeSpan? expiry,
    CancellationToken cancellationToken = default) Metodu**

Verilen anahtar ile veriyi Redis ??zerine yazma i??lemi i??in kullan??l??r.
Yazaca???? **T** tipindeki veriyi serile??tirme yaparak **JSON** string
olarak Redis ??zerine yazar.

??kinci parametrede verilen cancellationToken nesnesi redis i??lemini
gerekli durumlarda iptal edecektir. Redis i??lemi iptal oldu??unda ???Task
Canceled??? istisnas?? f??rlat??l??r.

Kullan??m ??ekli a??a????daki gibidir;

bool success = await SetAsync&lt;T&gt;(???KEY???, data,
TimeSpan.FromSeconds(100), cancellationToken);

-   **SetAsync(string key, string data, TimeSpan? expiry,
    CancellationToken cancellationToken = default) Metodu**

Verilen anahtar ile veriyi Redis ??zerine yazma i??lemi i??in kullan??l??r.
Yazaca???? **string** tipindeki veriyi Redis ??zerine yazar.

??kinci parametrede verilen cancellationToken nesnesi redis i??lemini
gerekli durumlarda iptal edecektir. Redis i??lemi iptal oldu??unda ???Task
Canceled??? istisnas?? f??rlat??l??r.

Kullan??m ??ekli a??a????daki gibidir;

bool success = await SetAsync(???KEY???, data, TimeSpan.FromSeconds(100),
cancellationToken);

-   **ClearAsync(CancellationToken cancellationToken, params
    string\[\] keys) Metodu**

Verilen anahtarlar?? Redis ??zerinden temizler ve ka?? adet temizlendi??ini
geriye d??nd??r??r.

??lk parametrede verilen cancellationToken nesnesi redis i??lemini gerekli
durumlarda iptal edecektir. Redis i??lemi iptal oldu??unda ???Task Canceled???
istisnas?? f??rlat??l??r.

Kullan??m ??ekli a??a????daki gibidir;

long deleteCount = await ClearAsync(cancellationToken, ???KEY1???, ???KEY2???,
???KEY3???);

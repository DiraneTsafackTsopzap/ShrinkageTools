
 Comment implementer une application Grpc service avec postgressql et dapper 

 Il est essentiel ici de comprendre d'abord comment est defini notre shrinkage_proto car elle stocke tous les

 Informations necessaires pour L'implementation , pour notre cas on va se baser sur ce que Lowell a fourni , mais ceci est

 dans le but pedagogique uniquement (https://dev.azure.com/lowell-dach/api/_git/api-cloud?path=%2Fprotos%2Flowell%2Fworkforce%2Fgrpc%2Fv1%2Fshrinkage_service.proto&_a=contents&version=GBmaster)




 I - Creer d'abord une Librairie et Dans cette Librairie on placera le DapperContext , Les Extentions , Mapper , Modeles , et les Fichiers protos , repositories et services

 1- Commencons d'abord par les Fichiers Protos


 Installer les dependances suivantes : 

 - Dapper 
 - Microsoft.Extensions.Configuration.Abstractions
 - Npgsql
 - Grpc.Tools  - Grpc.AspNetCore 

 S'assurer que les itemgroup soit present :

	<ItemGroup>
		<Protobuf Include="Protos\*.proto" GrpcServices="Both" />   Both veut dire ici qu'on aura le Serveur et le Client
	</ItemGroup>


 Ensuite 

  1.1 Protos -->> app_uuid.proto

     le app_uuid.proto est notre proto ou nous allons defini notre prope uuid ici

     Remarque 1 : Le nom du package ici est tres important car elle sera ensuite utiliser partout ( Voir Protos -->> app_uuid.proto)

     

 1.2  Protos --->> shrinkage_users.proto : est le Fichier qui sera utiliser tout le Long du Travail
      
      C'est ici que nous allons definir nos services , request et response


1.3  une fois le Lowell lien bien analyser on va donc commencer par Implementer le premier service qui est le GetUserByEmail
     
     - Le fichier shrinkage_proto ici comportera  2 choses primordials : 

        A-  Le service : Notre Service s'appelera  ShrinkageProtoService et cest a L'interieur qu'on placera toujours nos services
        
        B- ensuite on aura le Request et le Response

    Ca veut dire que pour implementer cela faudrai d'abord bien comprendre comment le response et le request ont ete definis

    Exple : On veut implementer le GetUserByEmail qui aura pour fonction de creer un user avec l'email si l'email n'existe pas , ou alors de retourner simplement l'user si l'email existe

Regarder bien le fichier shrinkage_users.proto comment les methodes ont ete definis , le request et la respnse associes

avant de commencer il serait Judicieux de se poser quelque questions pour la suite de notre implementation :
  
   -  Donner un nom au Service : ShrinkageProtoService

   -  Definir les Methodes pour ce service  : rpc GetUserByEmail (GetUserByEmailRequest) returns (GetUserByEmailResponse);

   - Comment est defini le uuid ici ? comment est definir le GetUserByEmailRequest , Ensuite le GetUserByEmailResponse

   -  y'a til des google.protobuf.duration ? ou de google.protobuf.timestamp ? le GetUserByEmailResponse retourne quoi ? un Objet , un string ou rien ?


   Tous ces Informations doivent etre bien assimiles au depart d'abord si on veut une bonne implementation


   Remarque 2 : - ne pas oublier l'import de notre namespace pour le AppUuid.proto (option csharp_namespace = "GrpcShrinkageServiceTraining.Protobuf";)

                -  ne pas oublier l'import de google/protobuf/duration.proto et celui de timestamp.proto

                - Exemple : import "google/protobuf/duration.proto"; et import "google/protobuf/timestamp.proto";

                -  Comprendre aussi les notions sur le go Protobuf , exemple : repeated PaidTime paid_time = 6; veut dire que l'user peut avoir plusieurs Paidstime


2- Creation des Modeles maintenants :
    
    - Definir un Modele de Donnees : Ds mon cas ici on a defini ShrinkageUserDataModel

    ici on doit aussi se poser les bonnes questions pour la suite : cest quoi le id primaire ? ya t'il des foreinkey ? ya til des id non null? yatil de date time? des floats , doubles etc ? 
    
    Dateonly ? , comprendre bien comment tous cela est definis


3- Implementation du DapperDbContext :
   
   - elle reste inchange , mais neamoins yaura des remarques a faire ici la , si j'ai deja un postgres.exe et pgadmin deja installer sur ma machine , alors

   pour eviter les conflicts avec le docker compose , deseactiver simplement les services en alleandt sur Services (Dienste ) --> chercher postgres et cliquer sur Beenden

   - Par contre si aucun postgres.exe ou pgadmin.exe est installer , alors tu peux lancer le docker compose

4- Definir les Extensions :

   Les deux Extensions utiles ici est le AppUuid.Partial.cs et le RPCExceptions

   - Remarque : ds la classe AppUuid.Partial.cs : faudrait faire Attention au namespace : elle doit correspondre a celui definis dans notre  app_uuid-proto  

   (namespace GrpcShrinkageServiceTraining.Protobuf ) si non cette classe sera rempli d'erreur 

   -  La classe RpcExceptions contient juste nos Exceptions


5- Definir ensuite nos Repositories ( Interface et classe )
   
   5.1 interface est : 

   -  Task<ShrinkageUserDataModel> Create(ShrinkageUserDataModel user, CancellationToken token);
   -  Task<ShrinkageUserDataModel?> GetUserByEmail(string email, CancellationToken token);
  
  5.2 Classe :

  -  Implementera les classes definis ds l'interface

   cest ici quon Appelera notre Dapper et on va ecrire les requetes SQL suivant les exigences requis


6- Definir Un Grand Service qui va etre utiliser dans le controller (Autre ASP NET API )

  Remarque ou piege a eviter ici : 

   - public class ShrinkageUsersGrpcService : ShrinkageProtoService.ShrinkageProtoServiceBase on voit bien que ShrinkageProtoService est le nom quon avait placer ds notre skrinkage_users.proto

   - Comprendre a present comment les methodes create et getuser ont ete creer ici , pour cela on doit d'abord comprendre comment les tables ds le sql ont ete definis

7- Comprendre les tables definis ds le fichiers init.sql

on constate ici que la table shrinkage_users n'a pas de foreinkey et la table skrinkage_user_paid_time a un foreinkey qui est le user id

- Notre Methode Create insere d'abord le User , Ensuite insere un User avec avec pour entree id , user_email , team_id , created_at et returne le id , user_email , team_id et le created_at

                                                Ensuite Insere le PaidTime de cet User , user_id = newuser.Id

                                                Enfin on fait un Merge pour retourner l'objet Shrinkage_UserDataModel



  8- comprendre bien les deux tables d'abord et comprendre comment chaque valeur est rempli et ou il est rempli :
                               




   




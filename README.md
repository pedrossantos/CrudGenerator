Projeto desenvolvido por Pedro Henrique da Silva Santos, com intuíto de divulgação de seus conhecimentos técnicos aplicados no desenvolvimento desta solução, CrudGenerator.

CrudGenerator é um aplicativo desenvolvido utilizando vários conceitos de boas práticas, utilizando tecnologias .NET e Code Obfuscation.
Como boas práticas, foram conceitos baseados na cultura SOLID: Clean Code, Design Patterns, Dependency Inversion, Micro Front-End, entre outros.
Tecnologias .NET Framework e .NET Core foram utilizados para o desenvolvimento de componentes de tela e também para o desenvolvimento da aplicação final, em WPF.
Para possibilitar a implementação desta solução, foi necerssário desenvolver também um Framework próprio, no qual existem implementaçãoes que vão desde a abstração de ORM (Object-Relational Mapping), também próprio, à abstrações para a camada de apresentação (View's).
Atualmente, o ORM desenvolvido dá suporte para leitura de definições de um determinado banco de dados, necessário para a geração de classes baseadas no Framework citado acima, e também, a partir de utilização do mesmo Framework, possibilita a implementação de modelos e repositórios em C# para interação com estes bancos de dados.

Hoje, temos suporte para bancos de dados Sqlite, SqlServer, PostgreSql e Mysql. Suporte para os bancos de dados Orable e Firebird estão em desenvolvimento.
Em paralelo, também está em andamento o desenvolvimento da geração da camada de aplicação e camada de apresentação para modelos e reposítórios criados à partir do CrudGenerator.

Também foi desenvolvido instaladores (pt-BR e en-US) para disponibilizar a aplicação para quem se interessar em conhecê-la. Para instaladores, acesse a pasta "installer/net462".
Caso queira saber mais sobre o Framework e esta solução, ou queira colaborar indicando ocorrência de bugs, entre em contato através do e-mail "ss.pedrohenrique@gmail.com".

Obrigado!

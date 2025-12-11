# TutoringPlatform
Esta plataforma foi desenvolvida para dar facilitar o todo o processo de gestão por parte dos serviços de tutoria da Universidade de Trás-os-Montes e Alto Douro.
### Requisitos
...

### Diagrama de classes
```mermaid
classDiagram
    Enrollement "n" <-- "1" Person

    Year "1" <-- "n" Enrollement
    Group "n" <-- "n" Enrollement
    Course "1" <-- "n" Enrollement

    Meeting "n" <-- "1" Group

    class Person{
        +int id
        +String firstName
        +Strnig lastName
        +String email
        +String phoneNumber
    }
    class Enrollement{
        +String role
    }
    class Year{
        +bool isActive
    }
    class Meeting{
        +Date date
        +String subject
        +String description
        +String typeContact
    }
```

# cimesrank

**cimesrank** es una API desarrollada en .NET par la empresa Cimes (JAEH) que gestiona rankings de recaudación y fiado para repartos. El sistema calcula, almacena y expone rankings mensuales, permitiendo comparar el desempeño de los repartos entre meses. Además, puede enviar reportes por email y ejecutar tareas automáticas en segundo plano.

## Características principales
- **Ranking de recaudación**: Calcula y almacena el ranking mensual de recaudación neta por reparto, comparando con el mes anterior.
- **Ranking de fiado**: Calcula y almacena el ranking mensual de fiado por reparto.
- **API REST**: Expone endpoints para consultar los rankings.
- **Tareas automáticas**: Un worker actualiza los rankings y otros servicios cada 24 horas.
- **Envío de reportes**: Permite enviar reportes por email a suscriptores.
- **Data**: Los valores son obtenidos por medio de stores procedures brindados por la empresa desarrolladora del software que utiliza embotelladora.

## Estructura del proyecto
- `Controllers/`: Endpoints de la API.
- `Core/Models/`: Modelos de datos principales.
- `Services/`: Lógica de negocio y servicios de ranking.
- `Infrastructure/`: Acceso a datos, email y reportes.
- `Workers/`: Tareas en segundo plano.

## Ejecución
1. Configura tus archivos `appsettings.json` y `appsettings.Development.json` con las cadenas de conexión y parámetros necesarios.
2. Restaura dependencias y compila:
   ```sh
   dotnet build cimesrank.sln
   ```
3. Ejecuta la aplicación:
   ```sh
   dotnet run --project ApiRanking.csproj
   ```

## Notas
- El proyecto utiliza .NET 8.0# cimesrank

# HolaMundo

Aplicación (`.NET MAUI`) que valida una contraseña ingresada por el usuario.

## Descripción

La aplicación muestra una pantalla con dos campos de entrada para la contraseña:

1. **Contraseña** – campo donde el usuario escribe su contraseña.
2. **Repetir contraseña** – campo para confirmar la contraseña ingresada.

Al presionar el botón **Validar**, la aplicación verifica que la contraseña cumpla con los siguientes requisitos:

- Al menos una letra mayúscula (`[A-Z]`)
- Al menos una letra minúscula (`[a-z]`)
- Al menos un dígito (`(?=.*\d)`)
- Al menos un carácter especial (`(?=.*[^a-zA-Z0-9])`)

Además, se comprueba que ambos campos de contraseña coincidan.

## Estructura del proyecto

```
HolaMundo/
├── HolaMundo/
│   ├── MainPage.xaml          # Interfaz de usuario (XAML)
│   ├── MainPage.xaml.cs       # Lógica de validación (code-behind)
│   ├── HolaMundo.csproj       # Configuración del proyecto .NET MAUI
│   └── Resources/             # Iconos, imágenes, fuentes y splash screen
└── README.md
```

## Detalles de implementación

### Interfaz (`MainPage.xaml`)

La página principal contiene los siguientes elementos:

| Elemento | Nombre en XAML | Comentario |
|---|---|---|
| `Label` | — | Título "Validación de contraseña" |
| `Label` | `PasswordRulesLabel` | Muestra las reglas que la contraseña debe cumplir. El texto se actualiza dinámicamente con el error encontrado durante la validación. |
| `Entry` | `PasswordEntry` | Campo de entrada para la contraseña. Usa `IsPassword="True"` para ocultar el texto. |
| `Entry` | `ConfirmPasswordEntry` | Campo de entrada para repetir la contraseña. También oculta el texto. |
| `Button` | `ValidateBtn` | Botón que, al ser presionado, llama al evento `OnValidateClicked` definido en el code-behind. |

### Lógica de validación (`MainPage.xaml.cs`)

- **`PasswordPattern`** – Patrón de expresión regular que define los requisitos de la contraseña:
  ```regex
  ^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+\z
  ```
- **`PasswordRegex`** – Instancia de `Regex` compilada.

#### `OnValidateClicked`

Evento principal del botón. Al hacer clic:

1. Llama a `ValidarFormulario()`.
2. Si la validación pasa, muestra una alerta con el mensaje **"La contraseña ha sido validada"**.

#### `ValidarFormulario`

Realiza dos comprobaciones:

1. **Regex** – Usa `Regex.IsMatch` para validar que la contraseña cumpla con todos los requisitos definidos en el patrón.
2. **Coincidencia** – Compara la contraseña ingresada con su confirmación usando `string.Equals` con `StringComparison.Ordinal`.

#### `MostrarError`

Método asincrónico que:

- Muestra una alerta al usuario con el mensaje de error.
- Actualiza el texto de `PasswordRulesLabel` para que el mensaje de error se muestre en pantalla.
- Anuncia el error a través del lector de pantalla con `SemanticScreenReader.Announce`.

## Cómo ejecutar

Ejecute la aplicación en la plataforma deseada (ejemplo en macOS):

   ```bash
   dotnet build                # Compila el proyecto
   dotnet run                  # Ejecuta en la plataforma local
   ```

## Requisitos

- .NET 10
- .NET MAUI
- Visual Studio 2022 17.12+ (recomendado) o Visual Studio Code (plugins)
- Xcode

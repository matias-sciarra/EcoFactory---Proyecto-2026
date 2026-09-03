# Notes & Advice Log

Running log of explanations/advice given by Claude. Newest at the bottom.

---

## 2026-09-03 — moneymachine.cs: CS0103 'dinero' does not exist

**Error:** `Assets\Scripts\Machine\moneymachine.cs(14,13): error CS0103: The name 'dinero' does not exist in the current context.`

**Cause:** `dinero` is a field declared on the `moneymanager` class ([moneymanager.cs](Assets/Scripts/manager/moneymanager.cs)), not on `moneymachine`. C# classes don't share fields just by being in the same project — `moneymachine` needs an explicit reference to a `moneymanager` instance to read/write its `dinero` field.

**Fix:** Added `public moneymanager manager;` to `moneymachine`, then used `manager.dinero += 100;` and updated `manager.txtdinero.text` accordingly.

**Action required in Unity Editor:** Select the GameObject with the `moneymachine` script and drag the GameObject holding `moneymanager` into the new `Manager` slot in the Inspector. If left empty, it will throw a `NullReferenceException` at runtime.

---

## 2026-09-03 — MachineSlot.cs: distancia lejos + tecla E para mejorar()

**Pedido:** agregar un `else if` para cuando el jugador está a más distancia, y que al presionar `E` se ejecute una función `mejorar()` (vacía por ahora, para completarla después).

**Cambios en `detectar()`:**
- El `if(distancia < distanciacerca)` original activa el cartel de mejora cuando el jugador está cerca.
- Se agregó `else if(distancia >= distanciacerca)` que pone `jugadorcerca = false` y oculta el cartel cuando el jugador está lejos.
- Se agregó un chequeo `if(jugadorcerca && Input.GetKeyDown(KeyCode.E)) mejorar();` para disparar la mejora solo si el jugador está en rango y presiona E.
- Se creó `public void mejorar() { }` vacía, lista para completar.

**Bugs preexistentes corregidos de paso (impedían compilar / funcionaban mal):**
- `using TMPro` le faltaba el `;` al final — error de compilación (CS1002).
- `if(distancia < distanciacerca);` tenía un `;` de más al final, lo que hacía que el bloque `{ }` de abajo se ejecutara siempre, sin importar la condición.
- `cartelmejora.SetActive;` no compila: `SetActive` es un método de `GameObject`/`Component`, no una propiedad, y necesita un argumento booleano. Se cambió a `cartelmejora.gameObject.SetActive(true)` (y `false` en el caso de lejos).

---

## 2026-09-03 — MachineSlot.cs: costo de mejora creciente vinculado a moneymanager.dinero

**Pedido:** vincular `mejorar()` al `dinero` de `moneymanager`, con un costo base público `costomejora`, donde el costo real crece como `costomejora * 1.6^cantidadMejoras`, y `cantidadMejoras` se incrementa con `+= 1` en cada mejora realizada.

**Cambios:**
- Nuevos campos en `MachineSlot`: `public float costomejora = 50f;` (costo base, ajustable en el Inspector) y `private int cantidadMejoras = 0;` (contador de mejoras ya realizadas).
- `mejorar()` ahora calcula `costoActual = costomejora * Mathf.Pow(1.6f, cantidadMejoras)`. Si `manager.dinero >= costoActual`, resta el costo de `manager.dinero`, actualiza `manager.txtdinero.text` y hace `cantidadMejoras += 1`. Si no alcanza el dinero, no pasa nada (no se descuenta ni se suma mejora).
- El contador se llamó `cantidadMejoras` (no `i`) para que sea legible como campo de clase; el incremento en sí es el `+= 1` pedido.
- Nota: cada mejora sube el costo un 60% sobre la anterior (factor 1.6), así que conviene revisar que `costomejora = 50f` sea el valor inicial que quieras — es fácil de cambiar desde el Inspector.

---

## 2026-09-03 — MachineSlot.cs: cartel como botón, processTime decreciente y detectar() en Update

**Pedido:** que el cartel de mejora sea un botón en vez de texto, que cada mejora reduzca `processTime` en 0.3333... segundos (1/3), y que `detectar()` se ejecute todos los frames desde un `void Update()`.

**Cambios:**
- `public TextMeshProUGUI cartelmejora;` → `public Button cartelmejora;` (agregado `using UnityEngine.UI;`). Ahora el Inspector va a pedir un componente `Button`, no un `TextMeshProUGUI` — hay que reasignar la referencia en la escena/prefab.
- Nuevo `void Start()` que hace `cartelmejora.onClick.AddListener(mejorar);`, así que clickear el botón también dispara la mejora (además de la tecla Q cuando el jugador está cerca, que se mantuvo).
- `cartelmejora.gameObject.SetActive(true/false)` sigue funcionando igual, porque `Button` también es un `Component` con `.gameObject`.
- Nuevo `void Update() { detectar(); }` — antes `detectar()` era público pero nada lo llamaba; ahora corre cada frame automáticamente. No había otro lugar del código llamándolo, así que no queda duplicado.
- En `mejorar()`, al concretarse la compra se agregó `processTime -= 1f / 3f;`.

**A tener en cuenta:** `processTime` no tiene un piso — después de suficientes mejoras puede llegar a 0 o negativo. No agregué un clamp porque no se pidió, pero si querés evitar que baje de cierto mínimo avisame y agrego un `Mathf.Max(...)`.

---

## 2026-09-03 — MachineSlot.cs: processTime con piso en 0 y texto del botón actualizado

**Pedido:** que `processTime` no pueda bajar de 0, y que el texto del botón de mejora (`txtcostomejora`, agregado por el usuario) muestre el costo actualizado después de cada mejora.

**Cambios en `mejorar()`:**
- `processTime -= 1f / 3f;` pasó a `processTime = Mathf.Max(0f, processTime - 1f / 3f);`, así nunca queda negativo.
- Después de incrementar `cantidadMejoras`, se recalcula el costo de la *próxima* mejora (`costomejora * Mathf.Pow(1.6f, cantidadMejoras)`) y se escribe en `txtcostomejora.text`. El valor inicial ya se seteaba en `Start()`, así que ahora el texto queda sincronizado en todo momento.

---

## 2026-09-03 — MachineSlot.cs: ocultar el botón de mejora si el jugador tiene algo agarrado

**Pedido:** que el botón de mejora no aparezca cuando el jugador tiene un pickable agarrado, para no sobrecargar la pantalla.

**Cambios:**
- `PlayerGrabber.cs`: se agregó `public bool IsHolding => heldItem != null;` — el campo `heldItem` era privado, así que no había forma de consultar desde afuera si el jugador tiene algo agarrado.
- `MachineSlot.cs`: se agregó `public PlayerGrabber jugador;` (asignar en el Inspector el objeto del jugador que tiene el `PlayerGrabber`).
- En `detectar()`, la condición para mostrar el cartel pasó de `if(distancia < distanciacerca)` a `if(distancia < distanciacerca && !jugador.IsHolding)`. El `else if(distancia >= distanciacerca)` se simplificó a un `else` liso, porque ahora la condición para "no mostrar" es la negación de una condición compuesta (lejos **o** con algo agarrado), no solo la distancia.

**Acción requerida en Unity:** arrastrar el GameObject del jugador (el que tiene `PlayerGrabber`) al nuevo campo `Jugador` del `MachineSlot` en el Inspector; si queda vacío tira `NullReferenceException`.

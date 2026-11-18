# Revisión estática del código (solo reporte, no se arregló nada)

Estado del código al 2026-11-18. Rutas relativas a `Assets/Scripts/`. Revisados los 27 scripts propios (no Standard Assets).

## 1. `return` que saltea trabajo en silencio cuando algo es null

| Dónde | Qué pasa |
|---|---|
| `Build/PlacementController.cs:41` | `Update` sale si falta `playerCamera` o `gridManager`: en modo construcción no pasa nada y no hay ningún log. |
| `manager/levelmanager.cs:35` | `Update` sale si falta `economymanager`: el nivel nunca sube, sin aviso. |
| `Build/BuildManager.cs:426` | `level()` devuelve `int.MaxValue` si no hay `levelmanager`: **todas** las máquinas quedan desbloqueadas, sin aviso. |
| `Build/BuildManager.cs:166` | `SeleccionarMaquina` sale si `maquinas` está vacío. |
| `Build/BuildManager.cs:201`, `:384` | `ActualizarPreview` / `ActualizarUIMaquina` salen sin avisar. |
| `Machine/machine.cs` `detectar()` (≈ línea 303), `AgarrarObjeto/MachineSlot.cs:157`, `generator/TrashGenerator.cs:105`, `cinta/Belt.cs:136` | `if (jugador == null) return;`: sin jugador no hay cartel ni mejora, sin log (solo `Start` avisa una vez en machine y MachineSlot). |
| `AgarrarObjeto/MachineSlot.cs:143-149` | `ProcessRoutine` espera una cinta libre para siempre y **no avisa** (machine.cs sí avisa). |
| `AgarrarObjeto/MachineSlot.cs:151` | Se ignora el `bool` que devuelve `SoltarEnCinta`: si falla, se consumieron 3 items y no sale nada (solo queda el log de error de `SoltarEnCinta`). |
| `Machine/machine.cs:296-297` | Igual: `salidaPendiente = false` aunque `SoltarEnCinta` haya fallado; se pierde el producto. |
| `economy/economymanager.cs:32-35` | `Gastar` devuelve false sin feedback; en `BuildManager.cs` el aviso de "plata insuficiente" es un TODO. |

## 2. Comparaciones que nunca pueden dar lo que parece

- No quedan comparaciones de un `int` con `null`. La de `levelmanager.Actualizartexto` (`currentLevel != null`) ya estaba corregida.
- `Build/BuildManager.cs:311`: `Mathf.Max(0, SiguienteDesbloqueda(0,1))` convierte "no hay ninguna máquina desbloqueada" (-1) en el índice 0, que puede estar bloqueada: se puede seleccionar y construir una máquina bloqueada.
- `Build/BuildManager.cs:20`: `CANTIDAD_MAQUINAS = 2` y el array por defecto de 2; en la escena hay 13 entradas. Solo importa si se resetea el componente.

## 3. `if` / `else` sin llaves con sangría engañosa

- `Build/BuildManager.cs:369`: `if (panelInfoMaquina != null)` y la línea siguiente `panelInfoMaquina.SetActive(false);` sin sangría. Funciona, pero parece que no pertenece al `if`.
- Los demás avisos del detector (`PlacementController.cs:91, 98, 133`, `Grabbable.cs:29`) son falsos positivos (condiciones en varias líneas o `if` de una sola línea).

## 4. Métodos con nombre de mensaje de Unity mal escrito

Ninguno. Todos los `Awake/Start/Update/OnEnable/OnDisable/OnApplicationQuit/OnDrawGizmos` están escritos exacto. Los únicos métodos que empiezan con "On"/"Start" y no son mensajes de Unity son `OnGrabbed`, `OnDropped`, `OnConsumedByMachine`, `StartPlacement`, `StartBeltMove`: son propios y bien llamados.

## 5. Llamadas caras en cada frame

| Dónde | Qué |
|---|---|
| `cinta/Belt.cs:141` (desde `Update`, línea 117) | `Physics.OverlapSphere` en **cada cinta, cada frame**; reserva un array nuevo cada vez (basura constante). |
| `Machine/MaquinaUtil.cs:13` (desde `machine.cs` `detectar()` y `MachineSlot.cs:159`) | Lo mismo para cada máquina y `MachineSlot`. |
| `economy/economymanager.cs:22` | `Update` reescribe el texto de la plata cada frame (`ToString` + asignación al TMP aunque no cambie). |
| `cinta/Belt.cs:125-128` | Si el destino está ocupado, `StartBeltMove` sale al toque y `Update` arranca **otra corrutina en el frame siguiente**, y así mientras esté bloqueada. |
| `generator/TrashGenerator.cs:45-51` | Cuando pasó el intervalo y la cinta sigue ocupada, `detectarbelt()` (un raycast) se ejecuta **todos los frames** porque el temporizador no se reinicia hasta que la cinta se libera. |
| `Build/PlacementController.cs:72-74` + `Build/GridCell.cs:84-98` | `SetValid()/SetInvalid()` se llaman cada frame y reasignan `renderer.material`. |
| `spawntest/spawnmanager.cs:39` | `FindGameObjectWithTag` en cada spawn (cada 10 s: aceptable, pero se podría guardar). |
| `Build/PlacementController.cs:45-52` | `Debug.Log` con interpolación en cada clic izquierdo del modo construcción (no es por frame, pero es ruido). |

`FindObjectOfType` solo aparece en `Awake`/`Start`/resolvers, nunca en `Update`. `GetComponent` por frame: no hay.

## 6. Suscripciones a eventos

- `BuildManager.OnConstruccionCambiada` (`Belt.cs:54`, `machine.cs`, `MachineSlot.cs:52`) y `BeltManager.OnPrecioMejoraCambiado` (`Belt.cs:55`): **todas** tienen su `-=` en `OnDisable`. Bien.
- `onClick.AddListener(mejorar)` en `Belt.cs:81`, `machine.cs:103`, `MachineSlot.cs:71`, `TrashGenerator.cs:40`: sin `RemoveListener`. Hoy no pierde nada porque el botón es hijo de la máquina; si algún botón fuera compartido, se acumularían listeners.

## 7. Corrutinas que pueden quedar colgadas

- No hay ninguna sin `yield` dentro de un bucle. `MachineSlot.cs:143` (`while (true)`) tiene `break` y `yield return null`.
- `AgarrarObjeto/MachineSlot.cs:138-153`: si el objeto se desactiva mientras procesa, Unity corta la corrutina y `isProcessing` queda en `true` **para siempre** (no se reinicia en `OnDisable` ni se retoma en `OnEnable`; `machine.cs` sí retoma). La máquina nunca vuelve a aceptar nada.
- `cinta/Belt.cs` `StartBeltMove`: el `while` mueve con `Velocidad * Time.deltaTime`; si `BeltManager.speed` se pone en 0 el item no llega nunca.

## 8. Campos públicos sin uso, y campos con el nombre de su tipo

- Sin uso en el código: `Machine/moneymachine.cs:17 dineroPorObjeto` (oculto en el Inspector), `manager/levelmanager.cs:15 nivel1`, `Trash/Trash.cs:15 trashType` y `:16 isHeld` (el script `Trash` no se usa en ningún lado salvo `machine.ReceiveTrash(Trash)`).
- Código muerto: `generator/TrashGenerator.cs:103 detectar()` **nunca se llama**; por eso el cartel y la mejora del generador (Q) no funcionan nunca. `manager/manager.cs` (tecla B) y `manager/cameramanager.cs` (tecla C) no están en la escena; el primero pelearía con `BuildManager` por la B si se agregara.
- Campo con el mismo nombre que su tipo: `manager/levelmanager.cs:9 public economymanager economymanager;`.
- `Camera/cameratopdown.cs:8` se llama `upset` (¿`offset`?).

## 9. Números fijos que deberían ser campos del Inspector

| Archivo:línea | Valor |
|---|---|
| `spawntest/spawnmanager.cs:11, 13-14, 43` | `spawnrate = 10`, `limitededestruibles = 5` (privados), altura `y = 1f` |
| `AgarrarObjeto/PlayerGrabber.cs:86` | distancia de soltar `1.2f` |
| `cinta/Belt.cs:49, 245` | velocidad de respaldo `2f`; incremento por mejora `1f / 3f` |
| `AgarrarObjeto/MachineSlot.cs:201` | reducción de `processTime` `1f / 3f` |
| `generator/TrashGenerator.cs:142` | `intervalo *= 0.8f` |
| `Machine/machine.cs:8` | `cantidadrequerida = 4` (privado) |
| `cinta/Belt.cs:29`, `TrashGenerator.cs:31` | multiplicador `2.1f`; `machine.cs:31` y `MachineSlot.cs:29` `1.6f` |
| `Build/BuildManager.cs:21, 36` | `1.7f` (precio) y `0.7f` (devolución) |
| `Build/PlacementController.cs:54, 386` | alcance de rayos `100f`, `50f` |
| `Build/Grid_Manager.cs:45`, `GridCell.cs:37, 44, 57` | `0.01f`, `1.2f`, alfa `0.15f` |
| `manager/savemanager.cs:40` | nombre `"partida.json"` |

## 10. Rayo de detección (DetectorConexion)

- `generator/TrashGenerator.cs:85-87` (`OnDrawGizmos`): **detecta primero con la dirección sin rotar y recién después la rota** para dibujar. El gizmo muestra una línea y la detección del gizmo usa otra. (`DetectarSalida()` real, línea 78-80, sí rota antes de detectar.)
- `cinta/Belt.cs:75`: `puntoSalida = transform.parent;` pisa el punto de salida asignado en el prefab. En edición el gizmo usa el punto asignado; en juego el rayo sale del padre (o de la cinta misma si no tiene padre). Gizmo y detección real **no coinciden**.
- `cinta/DetectorConexion.cs:20, 53`: `RaycastNonAlloc` con un buffer de **16** devuelve un subconjunto sin orden; con `distanciamax = 123` (MachineSlot, TrashGenerator) y muchos colliders en el camino, el primero real puede quedar afuera y la máquina no se conecta.
- `Belt` usa `r.EsValido` en el gizmo; machine/MachineSlot/TrashGenerator usan `r.belt != null`: el color verde/rojo no significa lo mismo entre ellos.

## 11. Otros hallazgos útiles

- `cinta/Belt.cs:143-157`: el cartel de mejora solo se actualiza **mientras el jugador está dentro del radio**; al alejarse queda prendido para siempre.
- `Machine/MaquinaUtil.cs:13-17`, `Belt.cs:141-147`, `TrashGenerator.cs:108-112`: la distancia se mide contra el collider con tag `Player`, que está en el objeto raíz `Player` que **no se mueve** (el que camina es el hijo `FPSController`).
- `Machine/MaquinaUtil.cs:36`: `PidioMejora` no tiene ninguna protección: un Q cobra la mejora en cada máquina y `MachineSlot` que estén en rango (las cintas ya lo resuelven en `BeltManager`).
- `Machine/machine.cs:114-115` y `AgarrarObjeto/MachineSlot.cs:79-80`: buscan el `Animator` en los hijos **después** de `ValidarAnimator` / `VolverAReposo`; la máquina no vuelve a reposo al arrancar.
- `AgarrarObjeto/MachineSlot.cs` nunca llama a `VolverAReposo` después de procesar.
- `AgarrarObjeto/Grabbable.cs:17-18, 29, 44`: `GetComponent<Collider>()` solo toma **un** collider del objeto raíz; los demás quedan activos mientras lo sostenés. Si `rb` es null, línea 27 lanza `NullReferenceException`.
- `AgarrarObjeto/PlayerGrabber.cs:30, 59`: si `rayOrigin` no está asignado, `NullReferenceException` al apretar E.
- `spawntest/spawnmanager.cs:68-73`: cuenta **colliders** con tag `destroyable`, no objetos (la botella tiene 2 BoxCollider). `:39-40`: si no hay objeto `plano` o no tiene `Renderer`, `NullReferenceException`.
- `manager/savemanager.cs:126, 147`: `WriteAllText` / `ReadAllText` / `FromJson` sin `try/catch`; un archivo roto o un disco lleno tira excepción. Además no se guardan las mejoras de máquinas ni de cintas.
- `Build/PlacementController.cs:81` vs `:170`: la preview usa la altura donde pega el mouse; la construcción usa la altura del piso en el centro de la celda.

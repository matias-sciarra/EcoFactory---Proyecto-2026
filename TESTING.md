# TESTING – checklist manual de EcoFactory

Esto es para seguir en el editor de Unity (2018.4), con la escena `escenademo` abierta. Como acá no hay consola de errores que te avise de todo, la idea es ir probando de a una cosa y mirar la **Console** (con los filtros de Warning y Error prendidos) mientras jugás.

Todo lo que dice "falla" es lo que se rompe en el código; al lado te dejo dónde mirar.

## Antes de arrancar (2 minutos)

1. Menú **Herramientas > Auditar proyecto** (sin darle Play). Te tira una línea por problema en la consola, con `[Auditor x]` adelante (la letra es el chequeo: a = scripts faltantes, b = items de cinta, c = items agarrables, d = conexiones, e = prefabs que se generan, f = animaciones, g = tags, h = escena, i = BuildManager, j = Build Settings). Hacé clic en la línea para seleccionar el objeto.
2. Arreglá los **errores** del auditor primero. Muchos de los tests de abajo no tienen sentido si el auditor te marca, por ejemplo, que no hay `savemanager` en la escena.
3. Tené a mano estos números reales del proyecto:
   - Plata inicial: **1000** (campo `dinero` del `economymanager`).
   - Niveles: nivel 2 con **2000**, nivel 3 con **10000**, nivel 4 con **50000**.
   - Máquinas con `machine`: necesitan **4** residuos, pagan **100** por cada uno. `MachineSlot` (1ra de cartón): **3** items, no paga.
   - Precio de construir = `precioBase × 1.7^veces compradas`. Vender devuelve el **70%** de lo que pagaste.
   - Mejora de cinta = `50 × 2.1^n`. Mejora de máquina = `300 × 1.6^n`. Se usan con **Q** estando a menos de 2 metros y sin nada en la mano.
   - Zona de compra: Carton 300, Plastico 500, Vidrio 800, Metal 1000, cualquier otro 100.

---

## 1. La escena arranca limpia

- **Qué hacés:** Play. No toques nada durante 15 segundos.
- **Qué tenés que ver:** la consola sin errores rojos ni warnings amarillos. Solo logs normales.
- **Si falla:** cada warning/error dice el nombre del objeto. Los típicos: "No se encontró el economymanager / PlayerGrabber" (falta algo en la escena), "No hay savemanager" (agregá un GameObject con `savemanager`), "estado de animación no encontrado" (nombres del Animator distintos a los del script).

## 2. Jugador y cursor

- **Qué hacés:** Play, sin clickear nada todavía. Intentá mover el mouse y caminar con WASD. Después apretá **Nueva partida**.
- **Qué tenés que ver:**
  - Con el menú abierto: el cursor **visible y libre**, los botones se pueden clickear, el jugador **no se mueve** ni la cámara gira, y B / E / Q no hacen nada.
  - Después de Nueva partida: cursor **bloqueado y escondido**, el jugador camina, corre con Shift, salta y mira con el mouse.
- **Si falla:** si no podés clickear los botones, el `MouseLook` del FirstPersonController le está volviendo a bloquear el cursor en cada frame (revisá que `MenuInicio` lo apague mientras el menú está abierto). Si el jugador se mueve detrás del menú, no se están desactivando los scripts del jugador.

## 3. Spawner de items

- **Qué hacés:** dejá correr el juego en nivel 1. Cada **10 segundos** tiene que aparecer un item. Con el objeto del `spawnmanager` seleccionado, mirá en el Inspector la lista `prefabs`.
  Después, con el juego corriendo, cambiá `dinero` del `economymanager` en el Inspector y repetí:
  - `dinero = 2000` (nivel 2): pueden salir los elementos 0 y 1.
  - `dinero = 10000` (nivel 3): 0, 1 y 2.
  - `dinero = 50000` (nivel 4): los 4.
- **Qué tenés que ver:**
  - Items que caen **sobre el piso** y se quedan ahí (no se hunden ni caen al vacío).
  - Nunca más de **5** items del spawner a la vez en el radio del spawner; si llegás a 5, deja de generar hasta que agarres o destruyas alguno.
  - Los tipos aparecen según el nivel (en nivel 1 solo el elemento 0).
  - El texto del nivel en pantalla coincide.
- **Si falla:**
  - Aparecen más de 5: el contador cuenta **colliders** con tag `destroyable`, no objetos. Si un prefab tiene 2 colliders (como la botella), cada uno suma 1 y el límite real es menor.
  - Nunca aparece nada: lista `prefabs` vacía o con entradas en null, o no hay objeto con tag `plano` (mirá la consola).
  - Salen de a más de uno por vez o salen objetos de otro tipo: revisá que los elementos de `prefabs` sean **prefabs del Project** y no copias de la Hierarchy.

## 4. El nivel no baja nunca

- **Qué hacés:** subí `dinero` a 2000 (nivel 2). Entrá al modo construcción (B) y comprá algo caro para que la plata baje de 2000. Salí con B.
- **Qué tenés que ver:** el nivel sigue en **2**. Las máquinas de nivel 2 siguen desbloqueadas en el menú de construcción y el spawner sigue sacando los items de nivel 2.
- **Si falla:** `levelmanager` está calculando el nivel a partir de la plata actual en vez de guardar el máximo alcanzado. También probá ver que no pase al cargar una partida guardada.

## 5. Agarrar, soltar y meter a mano

- **Qué hacés:**
  1. Mirá un item a menos de 3 metros y apretá **E**. Tiene que quedar en la mano.
  2. Apretá **E** mirando el piso: se tiene que soltar delante tuyo.
  3. Agarrá otro y apretá **E** mirando una máquina.
  4. Repetilo **para cada tipo de máquina** que acepte a mano: la 1ra de cartón (`MachineSlot`) y las `machine` que tengan `acceptedItemId` configurado (probá con el itemId correcto y con uno incorrecto).
- **Qué tenés que ver:**
  - Al agarrar: el item se ve **delante de la cámara** (no adentro) y no te empuja. En consola dice "agarrado con éxito".
  - Al soltar: cae al piso y se puede volver a agarrar.
  - Al meterlo en una máquina que lo acepta: el item desaparece, el contador sube (`actual` en el Inspector de `MachineSlot`; en `machine` el contador es privado, usá el modo Debug del Inspector), y la animación se reproduce **una vez**.
  - Con el itemId incorrecto o la máquina ocupada: el item **no** se consume y se suelta en el piso.
  - Cuando se completa el contador (3 en `MachineSlot`, 4 en `machine`): la máquina espera, y después aparece **un producto en la cinta de salida**.
  - Pago: `machine` suma **100** por item (se ve en el contador de plata). `MachineSlot` no paga.
- **Si falla:**
  - `NullReferenceException` en `Grabbable.cs` línea 27: el item no tiene Rigidbody.
  - "ese objeto no tiene el componente Grabbable / MachineSlot": el rayo pegó contra un collider hijo del modelo; tiene que buscar el componente en los padres.
  - La máquina `machine` no acepta nada a mano: revisá que tenga el método de carga a mano y `acceptedItemId` bien puesto.
  - La botella atraviesa el piso: el collider sólido del prefab está apagado y solo queda el trigger.

## 6. Items por cinta

- **Qué hacés:** armá (o abrí) una línea: generador/spawn → cinta → máquina → cinta de salida. Probá estos casos:
  1. **Una cinta** llevando items a una máquina.
  2. **Dos cintas** que entregan a la misma máquina.
  3. **Cinta de salida llena:** poné algo que bloquee la cinta de salida (o cortala) y dejá que la máquina complete su contador.
- **Qué tenés que ver:**
  - Caso 1: el item viaja por la cinta, entra en la máquina, se destruye, se paga y la animación corre.
  - Caso 2: los items entran **de a uno**; mientras la máquina está ocupada, el otro espera en su cinta (no se pisa ni desaparece).
  - Caso 3: la máquina queda **esperando** con el producto listo (en consola avisa una sola vez que no hay cinta libre / no está conectada). Cuando liberás la cinta de salida, el producto aparece. **No se pierde ningún producto.**
  - Un producto que sale de una máquina tiene que moverse solo por la cinta siguiente.
- **Si falla:**
  - La cinta no mueve nada: no hay `BeltManager` activo, o al item le falta `BeltItem`.
  - Una cinta queda "ocupada" para siempre: salió un producto sin `BeltItem` (pasaba con el Lingote). Mirá el error de consola con el nombre del prefab.
  - La cinta no conecta con la máquina: la capa del collider de la máquina no está en el `capasconectadas` de la cinta, o el punto de salida (`puntoSalida`) está mal. Prendé `debugConexiones` y mirá los gizmos con la escena en pausa.

## 7. Modo construcción

- **Qué hacés:** apretá **B** (entra), **R** (rota), clic izquierdo (construye), clic derecho sobre algo construido (vende). **Q/E** o teclas **1–9** cambian de máquina. Probá en este orden:
  1. Construir una máquina barata.
  2. Rotarla con R antes de construir otra y verificar la orientación.
  3. Construir otra del **mismo** tipo y mirar el precio.
  4. Intentar construir con la plata en 0 (bajá `dinero` en el Inspector).
  5. Vender con clic derecho.
  6. Mirar el menú con una máquina de nivel mayor al tuyo.
- **Qué tenés que ver:**
  - El precio sube **×1.7** por cada una comprada del mismo tipo, y **baja** cuando vendés (vuelve al valor anterior).
  - Vender te devuelve el **70%** de lo que pagaste, no del precio actual.
  - Sin plata: no se construye nada, el precio se pone **rojo** y la plata no cambia.
  - Las máquinas de nivel mayor aparecen **deshabilitadas** y no se pueden seleccionar con Q/E ni con los números.
  - **La vista previa (el "fantasma")** se ve en la celda bajo el mouse, **sin** carteles de mejora, **sin** conectarse a cintas y sin renombrarse ("Belt: 5" solo para las construidas de verdad). Las celdas ocupadas se pintan rojas y las libres verdes.
  - Al salir con B: volvés a la posición y rotación de antes, con el cursor bloqueado, y el techo vuelve a verse.
- **Si falla:**
  - Excepción al apretar B: falta una referencia en `BuildManager` (por ejemplo `techo` en otras escenas).
  - La preview muestra carteles o se conecta: el prefab de la preview está corriendo sus scripts.
  - Se construye con la plata en 0: mirá `TryComprarSeleccionada`.

## 8. Alturas: nada se hunde

- **Qué hacés:** construí máquinas en varias celdas, incluso en una donde el piso esté más alto o más bajo. Compará la altura del fantasma con la de la máquina ya construida. Con la escena en pausa, mirá desde el costado.
- **Qué tenés que ver:** la base de la máquina apoyada sobre el piso, ni flotando ni hundida, y la **preview a la misma altura** que la máquina definitiva.
- **Si falla:** la preview usa la altura donde pega el mouse y la construcción usa la altura del piso en el centro de la celda; si el piso no es plano, no coinciden. También puede ser el pivote del modelo (origen no en la base) o un `groundLayer` mal configurado.

## 9. Mejoras con Q

- **Qué hacés:** pará a menos de 2 m de una cinta **con la mano vacía** y apretá **Q** una vez. Repetí:
  1. Con **una** cinta cerca.
  2. Con **dos o más cintas** dentro del alcance.
  3. Con una cinta y una máquina juntas.
  4. Con una máquina sola (`machine` y `MachineSlot`).
- **Qué tenés que ver:**
  - El cartel de mejora aparece solo cuando estás cerca, tenés la mano vacía y no estás en modo construcción; **desaparece al alejarte**.
  - **Un Q = una compra.** Si hay varias cintas cerca, se cobra **solo la más cercana**.
  - El precio de la cinta es **compartido**: después de mejorar en una, todas las cintas muestran el precio nuevo (`50 × 2.1^n`), y la velocidad de todas sube.
  - Cada máquina tiene su propio precio (`300 × 1.6^n`) y su mejora: en `machine`, la animación se acelera; en `MachineSlot`, baja el tiempo de proceso. Si estás cerca de dos máquinas, tampoco se tendrían que cobrar las dos.
- **Si falla:**
  - El cartel no aparece nunca más lejos del lugar donde arrancó el jugador: el script está midiendo la distancia contra el objeto raíz `Player` (que no se mueve) en vez del jugador real.
  - El cartel se queda prendido al alejarte: la cinta solo lo actualiza mientras detecta al jugador dentro del radio.
  - Las mejoras de cintas y máquinas compiten juntas: un Q compra solo la más cercana de todas (`ArbitroMejoras`).

## 10. Guardar y cargar

- **Qué hacés:**
  1. **Nueva partida**, construí 2–3 máquinas (de distintos tipos), cambiá la plata y subí de nivel.
  2. Detené el Play y volvé a entrar.
  3. Mirá el menú: ¿el botón **Continuar** está activo?
  4. Apretá **Continuar**.
  5. Detené, volvé a entrar y apretá **Nueva partida**.
  6. Para ver el archivo: `%USERPROFILE%\AppData\LocalLow\<tu Company>\<tu Product>\partida.json` (los nombres están en Project Settings > Player).
- **Qué tenés que ver:**
  - Antes del primer guardado, **Continuar está deshabilitado** (o no hace nada). Después de jugar y salir, queda habilitado.
  - Al cargar: misma plata, mismo nivel, las mismas máquinas en las mismas celdas y con la misma rotación, y los **precios de construcción** al valor que corresponde por las compradas.
  - **Nueva partida** arranca con 1000 de plata, sin máquinas y nivel 1, y borra el archivo viejo.
  - Se guarda solo cada 60 segundos y al cerrar.
- **Si falla / lo que NO se guarda hoy:** las mejoras de máquinas y de cintas, y la velocidad de la cinta, **no** están en el archivo; al cargar vuelven a cero. Si no hay `savemanager` en la escena no se guarda ni se carga nada (el menú tiene que avisarlo en la consola). Un JSON roto puede tirar excepción al cargar.

## 11. Zona de compra (`moneymachine`)

- **Qué hacés:** mandá por cinta (o tirá hasta ahí) un item de cada material y fijate cuánto sube la plata. Después probá con un item con `tipoMaterial` **vacío** y uno con un material que **no esté en la tabla** (por ejemplo "Madera").
- **Qué tenés que ver:**
  - Carton +300, Plastico +500, Vidrio +800, Metal +1000. El item desaparece.
  - Material desconocido o vacío: paga **100** (`precioPorDefecto`) y aparece **un solo warning** por tipo, no uno por cada item.
- **Si falla:**
  - Todos pagan 100: los prefabs no tienen `tipoMaterial`, o la tabla `precios` está vacía.
  - El item llega y no pasa nada: la capa de la zona de compra no está en el `capasconectadas` de la cinta. (La zona se pasa sola a la capa `conexiones` al arrancar.)
  - El warning se repite en cada item: la lista de avisos no está funcionando.

## 12. Animaciones

- **Qué hacés:** mirá cada máquina con el juego corriendo (pausá y mirá en la ventana Animator con la máquina seleccionada cuál estado está activo).
- **Qué tenés que ver:**
  - Al arrancar, **todas** están en reposo (estado `New State`/`Idle`), ninguna anima sola.
  - Por cada item recibido, la animación (`Accion`) corre **una vez** completa, no se corta a la mitad y vuelve a reposo.
  - Al mejorar una `machine`, la animación se hace más rápida.
  - Una máquina sin clip igual procesa (usa 0.5 s de espera) y avisa **una sola vez** en la consola, con el nombre de la máquina.
- **Si falla:**
  - No anima nunca: el nombre del estado en el script no existe en el Animator Controller (por ejemplo `Procesar` vs `Accion`, o mayúsculas distintas). El auditor (chequeo `f`) lo marca.
  - Anima sola al aparecer: hay una transición sin condiciones desde el estado por defecto hacia el de proceso.
  - Se queda animando para siempre: el clip está en loop y el script no vuelve a reposo (`MachineSlot` nunca llama a volver a reposo después de procesar).

## 13. Extra: Build Settings y Play fuera del editor

- **Qué hacés:** File > Build Settings. Fijate qué escenas hay en la lista.
- **Qué tenés que ver:** `escenademo` en la lista y tildada. Con la lista vacía un build no incluye nada.
- **Si falla:** si te falta alguna escena el build abre una escena vacía o falla. El auditor (chequeo `j`) la marca.

---

## Cuando algo falla y no entendés por qué

1. Corré de nuevo **Herramientas > Auditar proyecto**: puede que ya te lo marque.
2. Fijate si hay un warning/error en la consola **justo antes** de lo que falló.
3. Seleccioná el objeto en pausa y mirá en el Inspector los campos que dependen de la escena (`manager`, `jugador`, `primeraCinta`, `capasconectadas`). Si están en `None` o en `Nothing`, ahí está.
4. Prendé `debugConexiones` en la cinta o máquina que no conecta y mirá los gizmos (rojo = no conecta, verde = conecta).

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProjectAuditor
{
    private const int NIVEL_MAXIMO_ALCANZABLE = 4;
    private const string ETIQUETA_DESTROYABLE = "destroyable";
    private const string ETIQUETA_JUGADOR = "Player";

    private enum Nivel
    {
        Aviso,
        Error
    }

    private static int errores;
    private static int avisos;
    private static int prefabsRevisados;
    private static int objetosDeEscena;
    private static int mascaraCintas;
    private static int mascaraEmisores;

    private static readonly Dictionary<char, int> porChequeo = new Dictionary<char, int>();
    private static readonly HashSet<string> yaDichos = new HashSet<string>();
    private static readonly HashSet<GameObject> prefabsGenerados = new HashSet<GameObject>();
    private static readonly HashSet<string> itemIdsGrabbables = new HashSet<string>();

    [MenuItem("Herramientas/Auditar proyecto")]
    public static void Auditar()
    {
        Reiniciar();

        try
        {
            List<GameObject> prefabs = CargarPrefabs();
            List<GameObject> raicesEscena = RaicesDeEscenasAbiertas();

            PrepararContexto(prefabs, raicesEscena);

            for (int i = 0; i < prefabs.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Auditando prefabs", prefabs[i].name, (float)i / Math.Max(1, prefabs.Count));
                AuditarRaiz(prefabs[i], false);
                prefabsRevisados++;
            }

            EditorUtility.DisplayProgressBar("Auditando escena", "objetos de la escena abierta", 0.9f);
            foreach (GameObject raiz in raicesEscena)
                AuditarRaiz(raiz, true);

            AuditarEtiquetas(raicesEscena);
            AuditarEscena(raicesEscena);
            AuditarBuildManager();
            AuditarBuildSettings();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Auditor] El auditor falló a mitad de camino: " + ex);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Resumen();
    }

    private static void Reiniciar()
    {
        errores = 0;
        avisos = 0;
        prefabsRevisados = 0;
        objetosDeEscena = 0;
        mascaraCintas = 0;
        mascaraEmisores = 0;
        porChequeo.Clear();
        yaDichos.Clear();
        prefabsGenerados.Clear();
        itemIdsGrabbables.Clear();
    }

    private static void Resumen()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[Auditor] Terminado: ");
        sb.Append(errores).Append(" error(es), ").Append(avisos).Append(" aviso(s). ");
        sb.Append("Prefabs revisados: ").Append(prefabsRevisados).Append(". Objetos de escena revisados: ").Append(objetosDeEscena).Append(". ");

        if (porChequeo.Count == 0)
        {
            sb.Append("No se encontró ningún problema.");
        }
        else
        {
            sb.Append("Por chequeo: ");
            List<char> claves = new List<char>(porChequeo.Keys);
            claves.Sort();
            foreach (char c in claves)
                sb.Append(c).Append("=").Append(porChequeo[c]).Append("  ");
        }

        sb.Append(" (filtrá la consola por \"[Auditor\" para ver solo esto)");
        Debug.Log(sb.ToString());
    }

    private static void Problema(Nivel nivel, char chequeo, UnityEngine.Object contexto, string mensaje)
    {
        string linea = "[Auditor " + chequeo + "] " + mensaje;
        if (!yaDichos.Add(linea))
            return;

        if (!porChequeo.ContainsKey(chequeo))
            porChequeo[chequeo] = 0;
        porChequeo[chequeo]++;

        if (nivel == Nivel.Error)
        {
            errores++;
            Debug.LogError(linea, contexto);
        }
        else
        {
            avisos++;
            Debug.LogWarning(linea, contexto);
        }
    }

    private static string Donde(GameObject go)
    {
        if (go == null)
            return "(null)";

        StringBuilder sb = new StringBuilder();
        Transform t = go.transform;
        List<string> partes = new List<string>();
        while (t != null)
        {
            partes.Add(t.name);
            t = t.parent;
        }
        partes.Reverse();
        sb.Append(string.Join("/", partes.ToArray()));

        if (EditorUtility.IsPersistent(go))
            return "Prefab '" + AssetDatabase.GetAssetPath(go) + "' > " + sb;

        return "Escena '" + go.scene.name + "' > " + sb;
    }

    private static List<GameObject> CargarPrefabs()
    {
        List<GameObject> lista = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (!ruta.StartsWith("Assets/"))
                continue;

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            if (go == null)
            {
                Problema(Nivel.Error, 'a', null, "No se pudo cargar el prefab '" + ruta + "' (archivo roto o con referencias imposibles de resolver)");
                continue;
            }

            lista.Add(go);
        }

        return lista;
    }

    private static List<GameObject> RaicesDeEscenasAbiertas()
    {
        List<GameObject> raices = new List<GameObject>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene escena = SceneManager.GetSceneAt(i);
            if (!escena.isLoaded)
                continue;

            raices.AddRange(escena.GetRootGameObjects());
        }

        return raices;
    }

    private static List<T> EnRaices<T>(List<GameObject> raices) where T : Component
    {
        List<T> lista = new List<T>();
        foreach (GameObject raiz in raices)
            lista.AddRange(raiz.GetComponentsInChildren<T>(true));
        return lista;
    }

    private static void PrepararContexto(List<GameObject> prefabs, List<GameObject> raicesEscena)
    {
        List<GameObject> todas = new List<GameObject>(prefabs);
        todas.AddRange(raicesEscena);

        foreach (GameObject raiz in todas)
        {
            foreach (Belt b in raiz.GetComponentsInChildren<Belt>(true))
                mascaraCintas |= b.capasconectadas.value;

            foreach (machine m in raiz.GetComponentsInChildren<machine>(true))
                mascaraEmisores |= m.capasconectadas.value;

            foreach (MachineSlot s in raiz.GetComponentsInChildren<MachineSlot>(true))
                mascaraEmisores |= s.capasconectadas.value;

            foreach (TrashGenerator g in raiz.GetComponentsInChildren<TrashGenerator>(true))
                mascaraEmisores |= g.capasconectadas.value;

            foreach (spawnmanager sm in raiz.GetComponentsInChildren<spawnmanager>(true))
            {
                if (sm.prefab != null)
                    prefabsGenerados.Add(sm.prefab);

                if (sm.prefabs != null)
                {
                    foreach (GameObject p in sm.prefabs)
                    {
                        if (p != null)
                            prefabsGenerados.Add(p);
                    }
                }
            }

            foreach (Grabbable g in raiz.GetComponentsInChildren<Grabbable>(true))
            {
                if (!string.IsNullOrEmpty(g.itemId))
                    itemIdsGrabbables.Add(g.itemId);
            }
        }
    }

    private static bool EsInstanciaDePrefab(GameObject go)
    {
        return !EditorUtility.IsPersistent(go) && PrefabUtility.GetCorrespondingObjectFromSource(go) != null;
    }

    private static void AuditarRaiz(GameObject raiz, bool enEscena)
    {
        foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
        {
            GameObject go = t.gameObject;
            if (enEscena)
                objetosDeEscena++;

            int vacios = 0;
            foreach (Component c in go.GetComponents<Component>())
            {
                if (c == null)
                    vacios++;
            }

            if (vacios > 0)
                Problema(Nivel.Error, 'a', go, vacios + " componente(s) con script faltante (Missing Script) en " + Donde(go));

            if (PrefabUtility.IsPrefabAssetMissing(go))
                Problema(Nivel.Error, 'a', go, "Instancia de prefab cuyo asset original ya no existe (Missing Prefab) en " + Donde(go));
        }

        foreach (MonoBehaviour mb in raiz.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb != null)
                BuscarReferenciasRotas(mb);
        }

        foreach (BeltItem bi in raiz.GetComponentsInChildren<BeltItem>(true))
            AuditarBeltItem(bi);

        foreach (Grabbable g in raiz.GetComponentsInChildren<Grabbable>(true))
            AuditarGrabbable(g);

        foreach (Belt b in raiz.GetComponentsInChildren<Belt>(true))
            AuditarConexion(b, b.capasconectadas.value, mascaraEmisores, true);

        foreach (MachineSlot s in raiz.GetComponentsInChildren<MachineSlot>(true))
        {
            AuditarConexion(s, s.capasconectadas.value, mascaraCintas, true);
            AuditarSalida(s, s.outputPrefab, "outputPrefab", true);
            AuditarAnimator(s, s.animator, s.animacion, LeerCadena(s, "estadoAnimacion", "Accion"), LeerCadena(s, "estadoReposo", "New State"));
            AuditarAcceptedItemId(s, s.acceptedItemId);
        }

        foreach (machine m in raiz.GetComponentsInChildren<machine>(true))
        {
            AuditarConexion(m, m.capasconectadas.value, mascaraCintas, true);
            AuditarSalida(m, m.residuoproscesado, "residuoproscesado", true);
            AuditarAnimator(m, m.animator, m.animacion, LeerCadena(m, "estadoAnimacion", "Accion"), LeerCadena(m, "estadoReposo", "New State"));
            AuditarAcceptedItemId(m, LeerCadena(m, "acceptedItemId", ""));
        }

        foreach (moneymachine mm in raiz.GetComponentsInChildren<moneymachine>(true))
            AuditarConexion(mm, -1, mascaraCintas, false);

        foreach (TrashGenerator tg in raiz.GetComponentsInChildren<TrashGenerator>(true))
        {
            if (tg.capasconectadas.value == 0)
                Problema(Nivel.Aviso, 'd', tg.gameObject, "capasconectadas está en Nothing en TrashGenerator: no se conecta con ninguna cinta. " + Donde(tg.gameObject));

            AuditarSalida(tg, tg.Prefabtrash, "Prefabtrash", true);
        }

        foreach (spawnmanager sm in raiz.GetComponentsInChildren<spawnmanager>(true))
            AuditarSpawner(sm);
    }

    private static void BuscarReferenciasRotas(MonoBehaviour mb)
    {
        if (mb.GetType().Assembly.GetName().Name != "Assembly-CSharp")
            return;

        SerializedObject so = new SerializedObject(mb);
        SerializedProperty it = so.GetIterator();

        while (it.NextVisible(true))
        {
            if (it.propertyType != SerializedPropertyType.ObjectReference)
                continue;

            if (it.objectReferenceValue == null && it.objectReferenceInstanceIDValue != 0)
                Problema(Nivel.Error, 'e', mb.gameObject, "Referencia rota (apunta a un asset o objeto que ya no existe) en " + mb.GetType().Name + "." + it.propertyPath + " de " + Donde(mb.gameObject));
        }
    }

    private static List<Collider> ColliderDe(Component c)
    {
        List<Collider> lista = new List<Collider>(c.GetComponentsInChildren<Collider>(true));
        foreach (Collider col in c.GetComponentsInParent<Collider>(true))
        {
            if (!lista.Contains(col))
                lista.Add(col);
        }
        return lista;
    }

    private static Rigidbody RigidbodyDe(Component c)
    {
        Rigidbody rb = c.GetComponentInChildren<Rigidbody>(true);
        if (rb != null)
            return rb;

        Rigidbody[] padres = c.GetComponentsInParent<Rigidbody>(true);
        return padres.Length > 0 ? padres[0] : null;
    }

    private static bool HaySolidoHabilitado(List<Collider> colliders)
    {
        foreach (Collider col in colliders)
        {
            if (col.enabled && !col.isTrigger)
                return true;
        }
        return false;
    }

    private static void AuditarBeltItem(BeltItem bi)
    {
        GameObject go = bi.gameObject;
        if (EsInstanciaDePrefab(go))
            return;

        Rigidbody rb = RigidbodyDe(bi);
        if (rb == null)
            Problema(Nivel.Aviso, 'b', go, "Item de cinta (BeltItem) sin Rigidbody: debería tener uno con isKinematic = true. " + Donde(go));
        else if (!rb.isKinematic)
            Problema(Nivel.Aviso, 'b', go, "Item de cinta (BeltItem) con Rigidbody NO kinematic: la gravedad lo va a sacar de la cinta, que lo mueve por transform. " + Donde(go));

        if (!HaySolidoHabilitado(ColliderDe(bi)))
            Problema(Nivel.Aviso, 'b', go, "Item de cinta (BeltItem) sin collider sólido habilitado (solo trigger o ninguno). " + Donde(go));

        if (string.IsNullOrEmpty(bi.tipoMaterial) || bi.tipoMaterial.Trim().Length == 0)
            Problema(Nivel.Aviso, 'b', go, "Item de cinta (BeltItem) con tipoMaterial vacío: la zona de compra va a pagar precioPorDefecto. " + Donde(go));

        Grabbable g = bi.GetComponent<Grabbable>();
        if (g != null)
            Problema(Nivel.Aviso, 'c', go, "Tiene BeltItem y Grabbable a la vez: solo se justifica si viaja por cinta hacia un MachineSlot (la cinta exige Grabbable ahí). Si no, sobra uno de los dos. " + Donde(go));

        GameObject raiz = go.transform.root.gameObject;
        bool esperabaGrabbable = prefabsGenerados.Contains(raiz) || raiz.name.IndexOf("va en la mano", StringComparison.OrdinalIgnoreCase) >= 0;
        if (g == null && esperabaGrabbable)
            Problema(Nivel.Error, 'c', go, "Tiene BeltItem pero se esperaba Grabbable (lo genera el spawnmanager o dice 'va en la mano'): el jugador no lo puede agarrar. " + Donde(go));
    }

    private static void AuditarGrabbable(Grabbable g)
    {
        GameObject go = g.gameObject;
        if (EsInstanciaDePrefab(go))
            return;

        Rigidbody rb = RigidbodyDe(g);
        if (rb == null)
            Problema(Nivel.Error, 'c', go, "Grabbable sin Rigidbody: al agarrarlo salta NullReferenceException en Grabbable.OnGrabbed (rb.isKinematic). " + Donde(go));

        List<Collider> colliders = ColliderDe(g);
        if (!HaySolidoHabilitado(colliders))
            Problema(Nivel.Aviso, 'c', go, "Grabbable sin collider sólido habilitado: atraviesa el piso y/o el rayo del jugador no lo agarra bien. " + Donde(go));

        if (rb != null && !rb.isKinematic)
        {
            foreach (Collider col in colliders)
            {
                MeshCollider mc = col as MeshCollider;
                if (mc != null && mc.enabled && !mc.convex)
                    Problema(Nivel.Error, 'c', go, "MeshCollider no convexo en un objeto con Rigidbody dinámico: PhysX lo rechaza. " + Donde(go));
            }
        }

        if (go.tag != ETIQUETA_DESTROYABLE)
            Problema(Nivel.Aviso, 'c', go, "Grabbable sin el tag '" + ETIQUETA_DESTROYABLE + "' (tiene '" + go.tag + "'): el spawnmanager no lo cuenta y genera objetos sin límite. " + Donde(go));

        if (string.IsNullOrEmpty(g.itemId))
            Problema(Nivel.Aviso, 'c', go, "Grabbable con itemId vacío: no coincide con ningún acceptedItemId. " + Donde(go));
    }

    private static void AuditarConexion(Component c, int mascaraPropia, int mascaraQueLoDetecta, bool exigirCapa)
    {
        GameObject go = c.gameObject;
        string tipo = c.GetType().Name;

        if (mascaraPropia == 0)
            Problema(Nivel.Aviso, 'd', go, "capasconectadas está en Nothing en " + tipo + ": no se conecta con nada. " + Donde(go));

        if (EsInstanciaDePrefab(go))
            return;

        List<Collider> habilitados = new List<Collider>();
        foreach (Collider col in c.GetComponentsInChildren<Collider>(true))
        {
            if (col.enabled)
                habilitados.Add(col);
        }

        if (habilitados.Count == 0)
        {
            Problema(Nivel.Aviso, 'd', go, tipo + " sin ningún collider habilitado (en sí mismo ni en hijos): las cintas no lo detectan. " + Donde(go));
            return;
        }

        if (!exigirCapa || mascaraQueLoDetecta == 0)
            return;

        foreach (Collider col in habilitados)
        {
            if ((mascaraQueLoDetecta & (1 << col.gameObject.layer)) != 0)
                return;
        }

        string capaNombre = LayerMask.LayerToName(habilitados[0].gameObject.layer);
        Problema(Nivel.Aviso, 'd', go, tipo + ": ninguno de sus colliders está en una capa que detecten los rayos (usa '" + capaNombre + "', las máscaras incluyen: " + NombresDeMascara(mascaraQueLoDetecta) + "). " + Donde(go));
    }

    private static string NombresDeMascara(int mascara)
    {
        List<string> nombres = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((mascara & (1 << i)) != 0)
            {
                string n = LayerMask.LayerToName(i);
                nombres.Add(string.IsNullOrEmpty(n) ? "capa " + i : n);
            }
        }
        return string.Join(", ", nombres.ToArray());
    }

    private static void AuditarAcceptedItemId(Component c, string acceptedItemId)
    {
        if (string.IsNullOrEmpty(acceptedItemId))
            return;

        if (!itemIdsGrabbables.Contains(acceptedItemId))
            Problema(Nivel.Aviso, 'c', c.gameObject, c.GetType().Name + " pide acceptedItemId '" + acceptedItemId + "' pero ningún Grabbable del proyecto tiene ese itemId: nunca va a aceptar nada a mano. " + Donde(c.gameObject));
    }

    private static void AuditarSalida(Component duenio, GameObject referencia, string campo, bool debeTenerBeltItem)
    {
        string tipo = duenio.GetType().Name;
        GameObject go = duenio.gameObject;

        if (referencia == null)
        {
            Problema(Nivel.Aviso, 'e', go, tipo + "." + campo + " está vacío: no genera nada. " + Donde(go));
            return;
        }

        VerificarReferenciaAPrefab(go, tipo + "." + campo, referencia);

        if (debeTenerBeltItem && referencia.GetComponent<BeltItem>() == null)
            Problema(Nivel.Aviso, 'e', go, tipo + "." + campo + " apunta a '" + referencia.name + "' que no tiene BeltItem en la raíz: la cinta no lo va a mover. " + Donde(go));
    }

    private static void VerificarReferenciaAPrefab(GameObject duenio, string campo, GameObject referencia)
    {
        if (!EditorUtility.IsPersistent(referencia))
            Problema(Nivel.Error, 'e', duenio, campo + " apunta a un objeto de la ESCENA ('" + referencia.name + "'), no a un prefab: si ese objeto se destruye queda en null. " + Donde(duenio));

        if (!referencia.activeSelf)
            Problema(Nivel.Aviso, 'e', duenio, campo + " apunta a '" + referencia.name + "' que está inactivo (activeSelf = false): si se instancia sin SetActive(true) no se ve ni funciona. " + Donde(duenio));
    }

    private static void AuditarSpawner(spawnmanager sm)
    {
        GameObject go = sm.gameObject;
        bool hayLista = sm.prefabs != null && sm.prefabs.Length > 0;

        if (!hayLista)
        {
            if (sm.prefab == null)
                Problema(Nivel.Aviso, 'e', go, "spawnmanager sin 'prefab' ni lista 'prefabs': no genera nada. " + Donde(go));
            else
                VerificarReferenciaAPrefab(go, "spawnmanager.prefab", sm.prefab);
            return;
        }

        for (int i = 0; i < sm.prefabs.Length; i++)
        {
            if (sm.prefabs[i] == null)
                Problema(Nivel.Aviso, 'e', go, "spawnmanager.prefabs[" + i + "] está vacío. " + Donde(go));
            else
                VerificarReferenciaAPrefab(go, "spawnmanager.prefabs[" + i + "]", sm.prefabs[i]);
        }

        if (sm.prefabs.Length < NIVEL_MAXIMO_ALCANZABLE)
            Problema(Nivel.Aviso, 'e', go, "spawnmanager.prefabs tiene " + sm.prefabs.Length + " elemento(s) pero hay " + NIVEL_MAXIMO_ALCANZABLE + " niveles: los niveles altos no suman objetos nuevos. " + Donde(go));
    }

    private static string LeerCadena(Component c, string campo, string porDefecto)
    {
        FieldInfo f = c.GetType().GetField(campo, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (f == null)
            return porDefecto;

        object valor = f.IsLiteral ? f.GetRawConstantValue() : f.GetValue(c);
        string texto = valor as string;
        return texto ?? porDefecto;
    }

    private static void AuditarAnimator(Component c, Animator asignado, AnimationClip clip, string nombreProceso, string nombreReposo)
    {
        GameObject go = c.gameObject;
        string tipo = c.GetType().Name;
        if (EsInstanciaDePrefab(go))
            return;

        Animator animator = asignado != null ? asignado : c.GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            Problema(Nivel.Aviso, 'f', go, tipo + " sin Animator (ni asignado ni en hijos): no se anima. " + Donde(go));
            return;
        }

        if (asignado == null)
            Problema(Nivel.Aviso, 'f', go, tipo + ".animator está vacío en el prefab (hay uno en '" + animator.name + "'): se completa recién al final de Start, después de volver a reposo. " + Donde(go));

        if (clip == null)
            Problema(Nivel.Aviso, 'f', go, tipo + ".animacion (AnimationClip) está vacío: el script lo completa en Start con el primer clip del controller, o usa un tiempo fijo. " + Donde(go));

        RuntimeAnimatorController rac = animator.runtimeAnimatorController;
        if (rac == null)
        {
            Problema(Nivel.Aviso, 'f', go, tipo + ": el Animator '" + animator.name + "' no tiene Animator Controller. " + Donde(go));
            return;
        }

        AnimatorOverrideController sobrescrito = rac as AnimatorOverrideController;
        while (sobrescrito != null)
        {
            rac = sobrescrito.runtimeAnimatorController;
            sobrescrito = rac as AnimatorOverrideController;
        }

        AnimatorController controller = rac as AnimatorController;
        if (controller == null || controller.layers.Length == 0)
        {
            Problema(Nivel.Aviso, 'f', go, tipo + ": no se pudo leer el controller del Animator (sin capas). " + Donde(go));
            return;
        }

        AnimatorStateMachine raiz = controller.layers[0].stateMachine;
        List<AnimatorState> estados = new List<AnimatorState>();
        foreach (ChildAnimatorState hijo in raiz.states)
            estados.Add(hijo.state);

        AnimatorState proceso = BuscarEstado(estados, nombreProceso);
        AnimatorState reposo = BuscarEstado(estados, nombreReposo);
        string existentes = NombresDeEstados(estados);

        if (proceso == null)
            Problema(Nivel.Aviso, 'f', go, tipo + ": el controller '" + controller.name + "' no tiene el estado '" + nombreProceso + "' en la capa 0 (hay: " + existentes + AyudaMayusculas(estados, nombreProceso) + "): la animación no se reproduce. " + Donde(go));

        if (reposo == null)
            Problema(Nivel.Aviso, 'f', go, tipo + ": el controller '" + controller.name + "' no tiene el estado de reposo '" + nombreReposo + "' en la capa 0 (hay: " + existentes + AyudaMayusculas(estados, nombreReposo) + "). " + Donde(go));

        if (proceso != null && proceso.motion == null)
            Problema(Nivel.Aviso, 'f', go, tipo + ": el estado '" + nombreProceso + "' no tiene ningún clip (Motion vacío). " + Donde(go));

        AnimatorState porDefecto = raiz.defaultState;
        if (porDefecto == null)
        {
            Problema(Nivel.Aviso, 'f', go, tipo + ": el controller '" + controller.name + "' no tiene estado por defecto: arranca sin animación definida. " + Donde(go));
        }
        else
        {
            if (proceso != null && porDefecto == proceso)
                Problema(Nivel.Aviso, 'f', go, tipo + ": el estado por defecto es el de proceso ('" + nombreProceso + "'): la máquina anima sola apenas aparece. " + Donde(go));

            foreach (AnimatorStateTransition t in porDefecto.transitions)
            {
                if (proceso != null && t.destinationState == proceso && t.conditions.Length == 0 && t.hasExitTime)
                    Problema(Nivel.Aviso, 'f', go, tipo + ": el estado por defecto '" + porDefecto.name + "' pasa solo a '" + nombreProceso + "' (transición sin condiciones y con Has Exit Time): la máquina anima sola. " + Donde(go));
            }
        }

        foreach (AnimatorStateTransition t in raiz.anyStateTransitions)
        {
            if (t.conditions.Length == 0)
                Problema(Nivel.Aviso, 'f', go, tipo + ": hay una transición desde Any State sin condiciones: el controller anima sin que nadie lo pida. " + Donde(go));
        }

        AnimationClip clipProceso = proceso != null ? proceso.motion as AnimationClip : null;
        if (proceso != null && clipProceso != null && proceso.transitions.Length == 0 && AnimationUtility.GetAnimationClipSettings(clipProceso).loopTime)
            Problema(Nivel.Aviso, 'f', go, tipo + ": el clip '" + clipProceso.name + "' está en loop y el estado '" + nombreProceso + "' no tiene transición de salida: si el script no fuerza el reposo, anima para siempre. " + Donde(go));
    }

    private static AnimatorState BuscarEstado(List<AnimatorState> estados, string nombre)
    {
        foreach (AnimatorState e in estados)
        {
            if (e.name == nombre)
                return e;
        }
        return null;
    }

    private static string NombresDeEstados(List<AnimatorState> estados)
    {
        List<string> nombres = new List<string>();
        foreach (AnimatorState e in estados)
            nombres.Add("'" + e.name + "'");
        return nombres.Count == 0 ? "ninguno" : string.Join(", ", nombres.ToArray());
    }

    private static string AyudaMayusculas(List<AnimatorState> estados, string nombre)
    {
        foreach (AnimatorState e in estados)
        {
            if (string.Equals(e.name, nombre, StringComparison.OrdinalIgnoreCase) && e.name != nombre)
                return "; ojo: existe '" + e.name + "', que difiere solo en mayúsculas";
        }
        return string.Empty;
    }

    private static Dictionary<string, string> EtiquetasUsadasEnCodigo(out HashSet<string> buscadas)
    {
        Dictionary<string, string> usadas = new Dictionary<string, string>();
        buscadas = new HashSet<string>();

        string carpeta = Path.Combine(Application.dataPath, "Scripts");
        if (!Directory.Exists(carpeta))
            return usadas;

        Regex patron = new Regex("(CompareTag|FindGameObjectWithTag|FindGameObjectsWithTag)\\s*\\(\\s*\"([^\"]+)\"");
        Regex comparacion = new Regex("\\.tag\\s*[!=]=\\s*\"([^\"]+)\"");

        foreach (string archivo in Directory.GetFiles(carpeta, "*.cs", SearchOption.AllDirectories))
        {
            string[] lineas = File.ReadAllLines(archivo);
            for (int i = 0; i < lineas.Length; i++)
            {
                foreach (Match m in patron.Matches(lineas[i]))
                {
                    string etiqueta = m.Groups[2].Value;
                    if (!usadas.ContainsKey(etiqueta))
                        usadas[etiqueta] = Path.GetFileName(archivo) + ":" + (i + 1);

                    if (m.Groups[1].Value.StartsWith("FindGameObject"))
                        buscadas.Add(etiqueta);
                }

                foreach (Match m in comparacion.Matches(lineas[i]))
                {
                    string etiqueta = m.Groups[1].Value;
                    if (!usadas.ContainsKey(etiqueta))
                        usadas[etiqueta] = Path.GetFileName(archivo) + ":" + (i + 1);
                }
            }
        }

        return usadas;
    }

    private static void AuditarEtiquetas(List<GameObject> raicesEscena)
    {
        HashSet<string> buscadas;
        Dictionary<string, string> usadas = EtiquetasUsadasEnCodigo(out buscadas);

        if (!usadas.ContainsKey(ETIQUETA_JUGADOR))
            usadas[ETIQUETA_JUGADOR] = "(requerida por el proyecto)";
        buscadas.Add(ETIQUETA_JUGADOR);

        string[] definidas = UnityEditorInternal.InternalEditorUtility.tags;
        List<Transform> todosLosTransform = new List<Transform>();
        foreach (GameObject raiz in raicesEscena)
            todosLosTransform.AddRange(raiz.GetComponentsInChildren<Transform>(true));

        foreach (KeyValuePair<string, string> par in usadas)
        {
            string etiqueta = par.Key;
            bool existe = Array.IndexOf(definidas, etiqueta) >= 0;

            if (!existe)
            {
                string parecida = null;
                foreach (string d in definidas)
                {
                    if (string.Equals(d, etiqueta, StringComparison.OrdinalIgnoreCase))
                        parecida = d;
                }

                string ayuda = parecida != null ? " (existe '" + parecida + "', que difiere solo en mayúsculas)" : string.Empty;
                Problema(Nivel.Error, 'g', null, "El código usa el tag '" + etiqueta + "' (" + par.Value + ") pero NO está definido en el TagManager" + ayuda + ": CompareTag lanza una excepción cada vez.");
                continue;
            }

            if (!buscadas.Contains(etiqueta))
                continue;

            List<GameObject> conEtiqueta = new List<GameObject>();
            foreach (Transform t in todosLosTransform)
            {
                if (t.gameObject.tag == etiqueta)
                    conEtiqueta.Add(t.gameObject);
            }

            if (conEtiqueta.Count == 0)
            {
                Problema(Nivel.Error, 'g', null, "Ningún objeto de la escena abierta tiene el tag '" + etiqueta + "' (el código lo busca en " + par.Value + ").");
                continue;
            }

            if (etiqueta != ETIQUETA_JUGADOR)
            {
                bool algunoSinRenderer = false;
                foreach (GameObject o in conEtiqueta)
                {
                    if (o.GetComponent<Renderer>() == null)
                        algunoSinRenderer = true;
                }

                if (algunoSinRenderer)
                    Problema(Nivel.Aviso, 'g', conEtiqueta[0], "Hay objetos con el tag '" + etiqueta + "' sin Renderer: el código hace GetComponent<Renderer>().bounds sobre el primero que encuentra y puede lanzar NullReferenceException. " + Donde(conEtiqueta[0]));
            }
        }
    }

    private static void ExigirUnico<T>(List<GameObject> raices, bool esError) where T : Component
    {
        List<T> lista = EnRaices<T>(raices);
        string nombre = typeof(T).Name;

        if (lista.Count == 0)
        {
            Problema(esError ? Nivel.Error : Nivel.Aviso, 'h', null, "La escena abierta no tiene ningún " + nombre + ".");
            return;
        }

        if (lista.Count > 1)
        {
            foreach (T extra in lista)
                Problema(Nivel.Error, 'h', extra.gameObject, "Hay " + lista.Count + " " + nombre + " en la escena (debería haber 1). " + Donde(extra.gameObject));
        }
    }

    private static bool EsControlDeJugador(Component c)
    {
        string n = c.GetType().Name;
        return n == "FirstPersonController" || n == "FPSController" || n == "RigidbodyFirstPersonController";
    }

    private static void AuditarEscena(List<GameObject> raices)
    {
        ExigirUnico<economymanager>(raices, true);
        ExigirUnico<levelmanager>(raices, true);
        ExigirUnico<BuildManager>(raices, true);
        ExigirUnico<PlacementController>(raices, true);
        ExigirUnico<Grid_Manager>(raices, true);
        ExigirUnico<savemanager>(raices, true);
        ExigirUnico<BeltManager>(raices, false);

        foreach (Transform t in EnRaices<Transform>(raices))
        {
            GameObject go = t.gameObject;
            if (go.tag != ETIQUETA_JUGADOR)
                continue;

            CharacterController propio = go.GetComponent<CharacterController>();
            if (propio != null)
            {
                foreach (CharacterController hijo in go.GetComponentsInChildren<CharacterController>(true))
                {
                    if (hijo != propio)
                        Problema(Nivel.Aviso, 'h', go, "El objeto con tag Player y uno de sus hijos ('" + hijo.name + "') tienen CharacterController: el de la raíz queda quieto como obstáculo invisible. " + Donde(go));
                }
            }

            bool seMueveElPropio = false;
            foreach (Component c in go.GetComponents<Component>())
            {
                if (c != null && EsControlDeJugador(c))
                    seMueveElPropio = true;
            }

            bool seMueveUnHijo = false;
            foreach (Component c in go.GetComponentsInChildren<Component>(true))
            {
                if (c != null && c.gameObject != go && EsControlDeJugador(c))
                    seMueveUnHijo = true;
            }

            if (!seMueveElPropio && seMueveUnHijo)
                Problema(Nivel.Aviso, 'h', go, "El tag Player está en un objeto que nunca se mueve: el que camina es un hijo. Todo lo que mide distancia contra el tag Player usa una posición que no cambia. " + Donde(go));
        }
    }

    private static void AuditarBuildManager()
    {
        List<GameObject> raices = RaicesDeEscenasAbiertas();

        foreach (BuildManager bm in EnRaices<BuildManager>(raices))
        {
            GameObject go = bm.gameObject;

            if (bm.maquinas == null || bm.maquinas.Length == 0)
            {
                Problema(Nivel.Error, 'i', go, "BuildManager sin máquinas configuradas. " + Donde(go));
                continue;
            }

            for (int i = 0; i < bm.maquinas.Length; i++)
            {
                BuildManager.MaquinaConstruible m = bm.maquinas[i];
                if (m == null)
                {
                    Problema(Nivel.Error, 'i', go, "BuildManager.maquinas[" + i + "] es null. " + Donde(go));
                    continue;
                }

                if (m.prefab == null)
                    Problema(Nivel.Error, 'i', go, "BuildManager.maquinas[" + i + "] no tiene prefab (vacío o referencia rota): se puede seleccionar pero no construir. " + Donde(go));
                else if (!EditorUtility.IsPersistent(m.prefab))
                    Problema(Nivel.Error, 'i', go, "BuildManager.maquinas[" + i + "].prefab apunta a un objeto de la escena ('" + m.prefab.name + "'), no a un prefab. " + Donde(go));

                if (m.imagenUI == null)
                    Problema(Nivel.Aviso, 'i', go, "BuildManager.maquinas[" + i + "] no tiene imagenUI: el panel de construcción muestra un cuadro vacío. " + Donde(go));

                if (m.nivelnecesario > NIVEL_MAXIMO_ALCANZABLE)
                    Problema(Nivel.Error, 'i', go, "BuildManager.maquinas[" + i + "].nivelnecesario = " + m.nivelnecesario + " pero levelmanager llega como mucho al nivel " + NIVEL_MAXIMO_ALCANZABLE + ": nunca se desbloquea. " + Donde(go));
            }
        }
    }

    private static void AuditarBuildSettings()
    {
        EditorBuildSettingsScene[] enBuild = EditorBuildSettings.scenes;
        HashSet<string> habilitadas = new HashSet<string>();
        foreach (EditorBuildSettingsScene s in enBuild)
        {
            if (s.enabled)
                habilitadas.Add(s.path);
        }

        if (enBuild.Length == 0)
            Problema(Nivel.Error, 'j', null, "Build Settings no tiene ninguna escena: un build no incluiría nada. Abrí File > Build Settings y agregá las escenas.");

        List<string> abiertas = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene escena = SceneManager.GetSceneAt(i);
            if (!string.IsNullOrEmpty(escena.path))
                abiertas.Add(escena.path);
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (!ruta.StartsWith("Assets/") || habilitadas.Contains(ruta))
                continue;

            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ruta);
            bool abierta = abiertas.Contains(ruta);
            Problema(abierta ? Nivel.Error : Nivel.Aviso, 'j', asset, "La escena '" + ruta + "' no está en Build Settings" + (abierta ? " (y es la que tenés abierta)" : string.Empty) + ".");
        }
    }
}

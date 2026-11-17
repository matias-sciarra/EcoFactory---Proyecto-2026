using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrecioMaterial
{
    public string tipo;
    public int precio;
}

public class moneymachine : MonoBehaviour
{
    public economymanager manager;

    [HideInInspector]
    public int dineroPorObjeto = 100;

    public PrecioMaterial[] precios = new PrecioMaterial[0];
    public int precioPorDefecto = 100;

    private static readonly HashSet<string> tiposAvisados = new HashSet<string>();
    private bool avisoSinManager;

    void Awake()
    {
        //Las maquinas construidas en runtime salen de un prefab, que no puede guardar
        //una referencia a un objeto de la escena: hay que buscarlo aca
        ResolverManager();
        AsignarCapaConexiones();
    }

    //Las cintas solo detectan colliders que esten en su LayerMask (capasconectadas, capa "conexiones").
    //Esta maquina estaba en Default, asi que el rayo de la cinta nunca la veia y no daba ningun error
    private void AsignarCapaConexiones()
    {
        int capa = LayerMask.NameToLayer("conexiones");

        if (capa < 0)
        {
            Debug.LogWarning(name + ": no existe la capa 'conexiones', las cintas no van a detectar esta maquina", this);
            return;
        }

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = capa;
    }

    private bool ResolverManager()
    {
        if (manager == null)
            manager = FindObjectOfType<economymanager>();

        if (manager == null && !avisoSinManager)
        {
            avisoSinManager = true;
            Debug.LogError(name + ": no hay economymanager en la escena, los objetos se destruyen sin pagar", this);
        }

        return manager != null;
    }

    //La cinta llama esto cuando el objeto llega a la maquina: paga y lo destruye
    public void RecibirObjeto(GameObject objeto)
    {
        if (objeto == null)
            return;

        BeltItem beltItem = BuscarBeltItem(objeto);
        if (beltItem != null)
        {
            RecibirObjeto(beltItem);
            return;
        }

        if (!objeto.scene.IsValid())
            return;

        AvisarUnaVez("(sin BeltItem)", name + ": el objeto " + objeto.name + " no tiene BeltItem, se paga precioPorDefecto (" + precioPorDefecto + ")");
        Pagar(precioPorDefecto);
        Destroy(objeto);
    }

    public void RecibirObjeto(BeltItem beltItem)
    {
        if (beltItem == null)
            return;

        GameObject raiz = RaizDelItem(beltItem);
        if (!raiz.scene.IsValid())
            return;

        Pagar(ObtenerPrecio(beltItem.tipoMaterial, raiz.name));

        Destroy(raiz);
        if (beltItem.item != null && beltItem.item != raiz && !beltItem.item.transform.IsChildOf(raiz.transform))
            Destroy(beltItem.item);
    }

    private static BeltItem BuscarBeltItem(GameObject objeto)
    {
        BeltItem beltItem = objeto.GetComponent<BeltItem>();
        if (beltItem == null)
            beltItem = objeto.GetComponentInParent<BeltItem>();
        if (beltItem == null)
            beltItem = objeto.GetComponentInChildren<BeltItem>(true);
        return beltItem;
    }

    private static GameObject RaizDelItem(BeltItem beltItem)
    {
        GameObject item = beltItem.item;
        if (item == null || item.transform.IsChildOf(beltItem.transform))
            return beltItem.gameObject;
        if (beltItem.transform.IsChildOf(item.transform))
            return item;
        return beltItem.gameObject;
    }

    private int ObtenerPrecio(string tipoMaterial, string nombreItem)
    {
        string clave = tipoMaterial == null ? "" : tipoMaterial.Trim();

        if (clave.Length > 0 && precios != null)
        {
            for (int i = 0; i < precios.Length; i++)
            {
                PrecioMaterial p = precios[i];
                if (p == null || p.tipo == null)
                    continue;

                if (string.Equals(p.tipo.Trim(), clave, System.StringComparison.OrdinalIgnoreCase))
                    return p.precio;
            }
        }

        if (clave.Length == 0)
            AvisarUnaVez("", name + ": el item " + nombreItem + " no tiene tipoMaterial, se paga precioPorDefecto (" + precioPorDefecto + ")");
        else
            AvisarUnaVez(clave.ToLowerInvariant(), name + ": el item " + nombreItem + " tiene tipoMaterial '" + tipoMaterial + "', que no esta en la tabla de precios. Se paga precioPorDefecto (" + precioPorDefecto + ")");

        return precioPorDefecto;
    }

    private void AvisarUnaVez(string clave, string mensaje)
    {
        if (tiposAvisados.Add(clave))
            Debug.LogWarning(mensaje, this);
    }

    private void Pagar(int cantidad)
    {
        if (!ResolverManager())
            return;

        manager.Ganar(cantidad);
    }
}

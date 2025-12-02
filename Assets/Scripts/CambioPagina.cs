using System.Collections.Generic;
using UnityEngine;

public class PanelPager : MonoBehaviour
{
    [Tooltip("Arrastra aquí cada 'página' (GameObjects) en orden. "
           + "Si lo dejas vacío, se llenará con los hijos directos de este objeto.")]
    public List<GameObject> pages = new List<GameObject>();

    [Tooltip("Página que se mostrará al iniciar (0 = primera).")]
    public int startIndex = 0;

    int index = 0;

    void Awake()
    {
        // Si no asignaste páginas en el Inspector, tomar hijos directos.
        if (pages.Count == 0)
        {
            pages = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
                pages.Add(transform.GetChild(i).gameObject);
        }
    }

    void Start()
    {
        Show(startIndex);
    }

    public void Show(int i)
    {
        if (pages == null || pages.Count == 0) return;

        index = Mathf.Clamp(i, 0, pages.Count - 1);
        for (int p = 0; p < pages.Count; p++)
            pages[p].SetActive(p == index);
    }

    public void NextPage()
    {
        if (pages.Count == 0) return;
        index = (index + 1) % pages.Count;
        Show(index);
    }

    public void PrevPage()
    {
        if (pages.Count == 0) return;
        index = (index - 1 + pages.Count) % pages.Count;
        Show(index);
    }
}

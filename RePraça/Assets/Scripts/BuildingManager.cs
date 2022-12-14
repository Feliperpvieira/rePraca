using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{

    //public GameObject[] objects; //lista de objetos - todos os objetos ficavam aqui e eles eram construídos pelo seu index
    public GameObject pendingObject; //objeto selecionado
    public GameObject painelObjetos; //painel com botoes e info pra adicionar objetos
    public GameObject botaoAddObjetos; //botao de adicionar novos objetos
    public GameObject interfaceTopoSistema; //botoes do topo da tela

    //[SerializeField] private Material[] materialPlacement; //materiais pra indicar por cor se pode ou não colocar um novo objeto ali - substituido por outline

    private Vector3 pos; //posição do obj
    private RaycastHit hit;

    [SerializeField] private LayerMask layerMask;

    public float rotateAmount;

    public float gridSize;
    bool gridOn;
    public bool canPlace = true;
    [SerializeField] private Toggle gridToggle;

    private SelectionManager selectionManager;
    private DiaNoite iluminacaoManager;

    public List<string> objetosPosicionados = new List<string>();


    void Start()
    {
        //coloca o objeto SelectManager da scene na variavel do codigo
        selectionManager = GameObject.Find("SelectManager").GetComponent<SelectionManager>();
        iluminacaoManager = GameObject.Find("IluminacaoManager").GetComponent<DiaNoite>(); //pega o script DiaNoite dentro do gameobject iluminacao manager

    }

    void Update()
    {
        if(pendingObject != null) //checa se existe um objeto selecionado
        {
            botaoAddObjetos.SetActive(false);
            UpdateMaterials(); //atualiza a cor pra definir se pode ou nao colocar lá

            if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer) //se for uma plataforma com touchscreen
            {
                if (!EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId) && Input.GetTouch(0).phase != TouchPhase.Ended) //checa se o toque esta batendo em um botao
                {
                    MoveObjectOnMap(); //se NAO estiver tocando num botao atualiza a posicao do objeto no mapa
                }
            
            }else if (!EventSystem.current.IsPointerOverGameObject()) //else se estiver no pc usa o ponteiro do mouse
            {
                MoveObjectOnMap(); //se o ponteiro do mouse NAO estiver sobre um botao atualiza a posicao do objeto no mapa
            }

            if (Input.GetKeyDown(KeyCode.P) && canPlace) //se apertar P & canPlace for true
            {
                PlaceObject();
            }

            if (Input.GetKeyDown(KeyCode.R)) //se apertar a tecla R ele gira o objeto
            {
                RotateObject();
            }
        }
        else if(pendingObject == null)
        {
            botaoAddObjetos.SetActive(true);
        }
    }

    void MoveObjectOnMap()
    {
        selectionManager.Select(pendingObject); //resseleciona o objeto a cada movimento pra impedir que acabe selecionando outro objeto durante a movimentação

        if (gridOn) //se a grid estiver ligada
        {
            //pega a posição de cada coord do mouse e arredonda elas
            pendingObject.transform.position = new Vector3(
                RoundToNearestGrid(pos.x),
                //RoundToNearestGrid(pos.y),
                pos.y,
                RoundToNearestGrid(pos.z)
                );
        }
        else //se a grid estiver desligada move o objeto livremente 
        {
            pendingObject.transform.position = pos; //movimenta o objeto
        }
    }

    public void PlaceObject()
    {
        //pendingObject.GetComponent<MeshRenderer>().material = materialPlacement[2]; //define a cor final ao posicionar o objeto
        if (canPlace)
        {
            objetosPosicionados.Add(pendingObject.name);
            
            pendingObject = null; //o objeto que estava selecionado não tá selecionado mais
            selectionManager.Deselect();
        }
        
    }

    public void RotateObject()
    {
        pendingObject.transform.Rotate(Vector3.up, rotateAmount); //up -> gira no y, rotateAmount -> variavel definida lá em cima
    }

    private void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //posição que vai colocar o objeto sendo segurado

        if(Physics.Raycast(ray, out hit, 1000, layerMask)) //esse 1000 é a distância que ele vai, pode trocar por uma variavel se quiser //layermask vai ser pra impedir que construa coisa sobre coisa
        {
            pos = hit.point; //o point pega o impact point no worldspace, basicamente diz pro jogo onde colocar o objeto
        }
    }
    
    void UpdateMaterials()
    {
        Outline outline = pendingObject.GetComponent<Outline>();
        if (canPlace)
        {
            //se canPlace for true, coloca o material 0 do array
            //pendingObject.GetComponent<MeshRenderer>().material = materialPlacement[0];
            outline.OutlineColor = Color.green;
        }
        if (!canPlace)
        {
            //se for false coloca o material 1
            //pendingObject.GetComponent<MeshRenderer>().material = materialPlacement[1];
            outline.OutlineColor = Color.red;
        }
    }

    public void SelectObject(GameObject objeto) //seleciona o objeto, é chamado pelo BotaoManager, o objeto é o prefab que ta no scriptable object dados
    {
        pendingObject = Instantiate(objeto, pos, transform.rotation);
        pendingObject.name = objeto.name;
        
        selectionManager.Select(pendingObject);
        //pendingObject.AddComponent<Outline>(); //não precisa mais adicionar o outline pq ele é adicionado no Select()
        Outline outline = pendingObject.GetComponent<Outline>();
        outline.OutlineColor = Color.green;
        outline.OutlineWidth = 5f;

        //materialPlacement[2] = pendingObject.GetComponent<MeshRenderer>().material; //coloca o material original do objeto como o usado pós posicionar - foi substituido pelo outline

        PainelAddObjetos();

        if (!iluminacaoManager.toggleNoiteDia.isOn) //se o toggleNoiteDia NÃO estiver on (!) então tá de noite
        {
            iluminacaoManager.AcendeOsPostes(); //acende os postes inclusive o recém adicionado (se tiver colocado um poste, se nao só reacende os velhos)
        }
    }

    public void ToggleGrid() //liga desliga a grid
    {
        if (gridToggle.isOn)
        {
            gridOn = true;
        }
        else
        {
            gridOn = false;
        }
    }

    float RoundToNearestGrid(float pos) //era usado quando tinha um botao de grid no app
    {
        float xDiff = pos % gridSize; //calcula o resto da posição pelo grid size

        //aí subtrai ou soma a posição pela diferença pra colocar a posição no grid mais próximo
        pos -= xDiff; 

        if(xDiff > (gridSize / 2))
        {
            pos += gridSize;
        }
        return pos;
    }

    public void PainelAddObjetos() //liga e desliga as coisas do painel de adicionar objetos
    {
        if(painelObjetos.activeInHierarchy == true)
        {
            interfaceTopoSistema.SetActive(true);
            painelObjetos.SetActive(false);
        }
        else if (painelObjetos.activeInHierarchy == false)
        {
            interfaceTopoSistema.SetActive(false);
            painelObjetos.SetActive(true);
        }
    }
}

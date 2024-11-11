using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using TMPro;
public class GameTextFlowSystem : MonoBehaviour
{
    public static GameTextFlowSystem instance;

    public xNodeText textNodeSystem;

    public TextNode currentNode;

    public List<Choise> currentChoises;

    public TMP_Text text;
    public TextNode nextTextNode;
    public NewSystemNode nextNodeSystem;

    public Choise choiseTemplate;
    public RectTransform choisesParent;

    public List<string> choiseKeys = new List<string>();

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
        Next(textNodeSystem.nodes[0] as TextNode);

    }

    public T Create<T>() where T:GameTextFlowSystem
    {
        T something = new GameTextFlowSystem() as T;

        return something;
    }


    public void Next (TextNode textNode)
    {
        for (int i = 0; i < currentChoises.Count; i++)
        {
            Destroy(currentChoises[i].gameObject);
        }
        currentChoises.Clear();

        if (textNode == null)
        {
            if (nextTextNode != null)
            {
                currentNode = nextTextNode;
            }
            else if (nextNodeSystem != null)
            {

                NewSystemNode newSystem = nextNodeSystem;
                TextNode nextNode = null;
                if (newSystem.startNode != "")
                {
                    Node node = newSystem.textNodeSystem.nodes.Find(c => c.name.ToLower() == newSystem.startNode.ToLower());
                    if (node != null && node.GetType() == typeof(TextNode))
                    {
                        nextNode = node as TextNode;
                    }
                }
                else
                {
                    Node node = newSystem.textNodeSystem.nodes[0];
                    if (node != null && node.GetType() == typeof(TextNode))
                    {
                        nextNode = node as TextNode;
                    }
                }

                currentNode = nextNode;
            }
            
        } else
        {
            currentNode = textNode;
            
        }
        nextTextNode = null;
        nextNodeSystem = null;
        text.text = currentNode.text;
        
        IEnumerable<NodePort> ports = currentNode.Ports;
        using (var sequenceEnum = ports.GetEnumerator())
        {
            while (sequenceEnum.MoveNext())
            {
                List<NodePort> nodePorts = sequenceEnum.Current.GetConnections();
                if (nodePorts != null && nodePorts.Count > 0)
                {
                    for (int i = 0; i < nodePorts.Count; i++)
                    {
                        

                        //Debug.Log((nodePorts[i].node as ChoiseNode).choiseText + ": " + nodePorts[i].direction);
                        if (nodePorts[i].direction == NodePort.IO.Input)
                        {
                            if (nodePorts[i].node.GetType() == typeof(TextNode))
                            {
                                nextTextNode = nodePorts[i].node as TextNode;
                                nextNodeSystem = null;
                                currentChoises.Add(choiseTemplate.Create(null, choisesParent));
                                
                            }
                            else if (nodePorts[i].node.GetType() == typeof(NewSystemNode))
                            {
                                nextNodeSystem = (nodePorts[i].node as NewSystemNode);
                                currentChoises.Add(choiseTemplate.Create(null, choisesParent));
                                nextTextNode = null;
                            }
                            else
                            {
                                nextTextNode = null;
                                nextNodeSystem = null;
                                currentChoises.Add(choiseTemplate.Create(nodePorts[i].node as ChoiseNode, choisesParent));

                            }

                        }

                    }
                }
            }
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}

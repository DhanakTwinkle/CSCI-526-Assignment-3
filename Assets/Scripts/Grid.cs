using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public struct node
    {
        public Vector2 location;
        public List<node> children;

        public node(Vector2 loc)
        {
            location = loc;
            children = new List<node>();
        }
    }    

    public static Grid instance = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public List<node> gridEntry;
    Dictionary<Vector2, node> lookup;
    public List<Vector2> groundChecks;

    Grid()
    {
        gridEntry = new List<node>();
        lookup = new Dictionary<Vector2, node>();

        node newnode = new node(new Vector2(0.0f, 0.0f));
        lookup.Add(new Vector2(0.0f, 0.0f), newnode);     
        gridEntry.Add(newnode);

        groundChecks = new List<Vector2>();
        groundChecks.Add(new Vector2(0.0f, 0.0f));
    }

    public void add(Vector2 v)
    {
        node newNode = new node(v);
        Vector2 location;
        for(int x = -1; x<=1; x++)
        {
            for(int  y = -1; y<=1; y++)
            {
                if (x == y)
                    continue;
                if ((x == -1 && y == 1) || (x == 1 && y == -1))
                    continue;

                location = v;
                location.x += x;
                location.y += y;
                if (lookup.ContainsKey(location))
                {
                    lookup[location].children.Add(newNode);
                    newNode.children.Add(lookup[location]);
                }
            }
        }

        lookup.Add(v, newNode);
        gridEntry.Add(newNode);

        Vector2 loc = v + new Vector2(0.0f, 1.0f);
        groundChecks.Remove(loc);

        loc = v - new Vector2(0.0f, 1.0f);
        if(!lookup.ContainsKey(loc))
            groundChecks.Add(v);
    }

    public void remove(Vector2 v)
    {
        Vector2 location;
        node deletingNode = lookup[v];
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == y)
                    continue;
                if ((x == -1 && y == 1) || (x == 1 && y == -1))
                    continue;

                location = v;
                location.x += x;
                location.y += y;
                if (lookup.ContainsKey(location))
                {
                    lookup[location].children.Remove(deletingNode);
                }
            }
        }

        if(groundChecks.Contains(v))
        {
            groundChecks.Remove(v);
            Vector2 loc = v + new Vector2(0.0f, 1.0f);
            if (lookup.ContainsKey(loc))
                groundChecks.Add(loc);
        }

        gridEntry.Remove(lookup[v]);
        lookup.Remove(v);
    }

    public bool check(Vector2 v)
    {
        if (lookup.ContainsKey(v))
            return true;
        else
            return false;
    }

    public bool check(Vector2 v, out bool deletable)
    {
        if(lookup.ContainsKey(v))
        {
            if (lookup.Count == 1)
            {
                deletable = false;
                return true;
            }
            if (lookup.Count == 2)
            {
                deletable = true;
                return true;
            }
            Vector2 start = gridEntry[0].location == lookup[v].location? gridEntry[1].location: gridEntry[0].location;
            List<Vector2> visited = new List<Vector2>();
            
            node delete = lookup[v];
            if(delete.children.Count == 4)
            {
                deletable = false;
                return true;
            }

            dfs((int)start.x, (int)start.y, ref delete, ref visited);
            
            if (visited.Count > 1 && visited.Count + 1 == gridEntry.Count)
                deletable = true;
            else
                deletable = false;
            return true;
        }
        else
        { 
            deletable = false;
            return false;
        }
    }

    void dfs(int x, int y, ref node deletable, ref List<Vector2> visited)
    {
        Vector2 loc = new Vector2((int)x, (int)y);
        if (visited.Contains(loc))
            return;
        if (loc == deletable.location)
            return;
        if (!lookup.ContainsKey(loc))
            return;

        visited.Add(loc);
        
        dfs(x + 1, y, ref deletable, ref visited);
        dfs(x - 1, y, ref deletable, ref visited);
        
        dfs(x, y + 1, ref deletable, ref visited);
        dfs(x, y - 1, ref deletable, ref visited);
    }
}

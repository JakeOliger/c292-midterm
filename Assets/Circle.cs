using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Circle : Enemy
{
    private List<Tuple<Circle, LineRenderer>> _conns = new List<Tuple<Circle, LineRenderer>>();
    LineRenderer _linePrefab;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        bounty = 25;
    }

    public void SetLineRendererPrefab(LineRenderer prefab) {
        _linePrefab = prefab;
    }

    public void AddConnection(Circle connection) {
        if (!connection.IsConnection(this) && _conns.Count < 2) {
            LineRenderer lr = Instantiate(_linePrefab, transform);
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, connection.transform.position);
            Vector3 pos = transform.position;
            pos.z = -1;
            lr.transform.position = pos;
            _conns.Add(new Tuple<Circle, LineRenderer>(connection, lr));
        }
    }

    public void removeConnection(Circle connection) {
        for (int i = 0; i < _conns.Count; i++) {
            if (_conns[i].Item1 == connection) {
                _conns.RemoveAt(i);
                return;
            }
        }
    }

    public void EmptyConnections() {
        _conns.Clear();
    }

    public bool IsConnection(Circle connection) {
        foreach (Tuple<Circle, LineRenderer> conn in _conns) {
            if (conn.Item1 == connection)
                return true;
        }
        return false;
    }

    public List<Circle> GetConnections() {
        List<Circle> circles = new List<Circle>();
        foreach (Tuple<Circle, LineRenderer> conn in _conns) {
            if (conn.Item1 != null)
                circles.Add(conn.Item1);
        }
        return circles;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        // Update lines
        for (int i = 0; i < _conns.Count; i++) {
            if (_conns[i].Item1 == null) {
                Destroy(_conns[i].Item2);
                _conns.RemoveAt(i);
                i--;
            } else {
                _conns[i].Item2.SetPosition(0, transform.position);
                _conns[i].Item2.SetPosition(1, _conns[i].Item1.transform.position);
            }
        }
    }
}

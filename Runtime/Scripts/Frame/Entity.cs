using System.Collections;
using System.Collections.Generic;
using System;

namespace KmaxXR
{

    public delegate void Callback();

    public delegate void Callback<T>(T arg1);

    public delegate void Callback<T, U>(T arg1, U arg2);

    public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);

    public abstract class Entity
    {

        private readonly Dictionary<Enum, Delegate> _table = new Dictionary<Enum, Delegate>();

        //多种AddDelegate，每一种附带不同数量参数
        public void AddListener(Enum p_type, Callback pListener)
        {
            _table[p_type] = _table.ContainsKey(p_type) ? Delegate.Combine(_table[p_type], pListener) : pListener;
        }

        public void AddListener<T>(Enum p_type, Callback<T> pListener)
        {
            _table[p_type] = _table.ContainsKey(p_type) ? Delegate.Combine(_table[p_type], pListener) : pListener;
        }

        public void AddListener<T, U>(Enum p_type, Callback<T, U> pListener)
        {
            _table[p_type] = _table.ContainsKey(p_type) ? Delegate.Combine(_table[p_type], pListener) : pListener;
        }

        public void AddListener<T, U, V>(Enum p_type, Callback<T, U, V> pListener)
        {
            _table[p_type] = _table.ContainsKey(p_type) ? Delegate.Combine(_table[p_type], pListener) : pListener;
        }

        public void RemoveListener(Enum pType, Callback pListener)
        {
            if (_table.ContainsKey(pType))
            {
                Delegate d = Delegate.Remove(_table[pType], pListener);
                if (d == null) _table.Remove(pType);
                else _table[pType] = d;
            }
        }

        public void RemoveListener<T>(Enum pType, Callback<T> pListener)
        {
            if (_table.ContainsKey(pType))
            {
                Delegate d = Delegate.Remove(_table[pType], pListener);
                if (d == null) _table.Remove(pType);
                else _table[pType] = d;
            }
        }

        public void RemoveListener<T, TU>(Enum pType, Callback<T, TU> pListener)
        {
            if (_table.ContainsKey(pType))
            {
                Delegate d = Delegate.Remove(_table[pType], pListener);
                if (d == null) _table.Remove(pType);
                else _table[pType] = d;
            }
        }

        public void RemoveListener<T, TU, TV>(Enum pType, Callback<T, TU, TV> pListener)
        {
            if (_table.ContainsKey(pType))
            {
                Delegate d = Delegate.Remove(_table[pType], pListener);
                if (d == null) _table.Remove(pType);
                else _table[pType] = d;
            }
        }

        /// <summary>
        ///     Clear all listeners of a given type.
        /// </summary>
        /// <param name="p_type">Enum that describes the event being listened.</param>
        public void RemoveAllListeners(Enum p_type)
        {
            _table.Remove(p_type);
        }


        public void Dispatch(Enum eventType)
        {
            if (!_table.ContainsKey(eventType)) return;

            Delegate delega = _table[eventType];

            if (delega != null)
            {
                var callback = delega as Callback;
                if (callback != null) callback();
            }
        }

        public void Dispatch<T>(Enum pType, T arg1)
        {
            if (!_table.ContainsKey(pType)) return;

            Delegate delega = _table[pType];

            if (delega != null)
            {
                var callback = delega as Callback<T>;
                if (callback != null) callback(arg1);
            }
        }

        public void Dispatch<T, TU>(Enum pType, T arg1, TU arg2)
        {
            if (!_table.ContainsKey(pType)) return;

            Delegate delega = _table[pType];

            if (delega != null)
            {
                var callback = delega as Callback<T, TU>;
                if (callback != null) callback(arg1, arg2);
            }
        }

        public void Dispatch<T, TU, TV>(Enum pType, T arg1, TU arg2, TV arg3)
        {
            if (!_table.ContainsKey(pType)) return;

            Delegate delega = _table[pType];

            if (delega != null)
            {
                var callback = delega as Callback<T, TU, TV>;
                if (callback != null) callback(arg1, arg2, arg3);
            }
        }

    }

    public class GlobalEntity : Entity
    {
        private static GlobalEntity _instance;
        public static GlobalEntity Instance
        {
            get
            {
                if (_instance == null) _instance = new GlobalEntity();
                return _instance;
            }
        }

        private GlobalEntity() { }
    }

}
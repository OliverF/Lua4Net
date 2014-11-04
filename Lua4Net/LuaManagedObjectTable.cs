namespace Lua4Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    using Lua4Net.Types;

    public class LuaManagedObjectTable : LuaManagedObjectTable<LuaManagedObjectTable>
    {
    }

    public abstract class LuaManagedObjectTable<TSubTable> : LuaManagedObject
        where TSubTable : LuaManagedObjectTable<TSubTable>, new()
    {
        private readonly IDictionary<LuaValueType, LuaManagedObjectTableField> tableFields;

        protected LuaManagedObjectTable()
        {
            this.tableFields = new Dictionary<LuaValueType, LuaManagedObjectTableField>();
        }

        public int FieldCount
        {
            get
            {
                return this.GetTableFieldCount();
            }
        }

        public LuaManagedObjectTableField GetField(LuaValueType key)
        {
            var field = this.GetTableFieldIfKeyExists(key);

            if (field == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Table key \"{0}\" doesn't exist.", key));
            }

            return field;
        }

        public LuaValueType GetValueField(LuaValueType key)
        {
            var field = this.GetField(key);

            var valueField = field as LuaManagedObjectTableValueField;

            if (valueField == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Table field with key \"{0}\" is not a value field.", key));
            }

            return valueField.Value;
        }

        public TSubTable GetSubTableField(LuaValueType key)
        {
            var field = this.GetField(key);

            var valueField = field as LuaManagedObjectSubTableField<TSubTable>;

            if (valueField == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Table field with key \"{0}\" is not a sub table field.", key));
            }

            return valueField.Value;
        }

        public IDictionary<LuaValueType, LuaManagedObjectTableField> FetchTableFields()
        {
            return this.CopyFieldsDictionary();
        }

        public void Clear()
        {
            this.ClearTableFields();
        }

        protected internal override void RemoveLuaReference(Lua lua)
        {
            base.RemoveLuaReference(lua);

            this.RemoveLuaReferenceOfAllSubTables(lua);
        }

        protected override void IndexMetamethod(Lua lua, LuaTable table, LuaType key)
        {
            var oldStackCount = lua.Stack.Count;

            var keyAsValueType = CheckAndConvertTableKey(key);

            this.IndexPushTableFieldToStack(lua, keyAsValueType);

            Debug.Assert(lua.Stack.Count == oldStackCount + 1, "stack count");
        }

        protected virtual void IndexPushTableFieldToStack(Lua lua, LuaValueType key)
        {
            var field = this.GetTableFieldIfKeyExists(key);

            if (field == null)
            {
                // if the key doesn't exist => return nil (like real lua tables)
                lua.Stack.PushValue(new LuaNilValue());
            }
            else
            {
                var valueField = field as LuaManagedObjectTableValueField;
                var subTableField = field as LuaManagedObjectSubTableField<TSubTable>;

                if (valueField != null)
                {
                    lua.Stack.PushValue(valueField.Value);
                }
                else if (subTableField != null)
                {
                    subTableField.Value.CreateMetatableAndPushToStack(lua);
                }
            }
        }

        protected override void NewIndexMetamethod(Lua lua, LuaTable table, LuaType key, LuaType value)
        {
            var keyAsValueType = CheckAndConvertTableKey(key);
            var newField = this.NewIndexValueToTableField(lua, keyAsValueType, value);

            if (newField == null)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Cannot assign \"{0}\" to managed object table.", value);
                throw new LuaException(msg);
            }

            this.SetTableField(keyAsValueType, newField);
        }

        protected virtual LuaManagedObjectTableField NewIndexValueToTableField(Lua lua, LuaValueType key, LuaType value)
        {
            var valueAsValueType = value as LuaValueType;

            if (valueAsValueType != null)
            {
                LuaManagedObjectTableField tableField = new LuaManagedObjectTableValueField(valueAsValueType);
                return tableField;
            }

            var valueAsTable = value as LuaTable;

            if (valueAsTable != null)
            {
                if (valueAsTable.GetLength(LuaTableGetLengthMethod.LuaNextCount) != 0)
                {
                    throw new LuaException("Cannot assign a non-empty table to a managed object table.");
                }

                var subTable = Activator.CreateInstance<TSubTable>();
                subTable.AddLuaReference(lua);

                LuaManagedObjectTableField tableField = new LuaManagedObjectSubTableField<TSubTable>(subTable);
                return tableField;
            }

            return null;
        }

        private static LuaValueType CheckAndConvertTableKey(LuaType value)
        {
            var valueAsValueType = value as LuaValueType;

            if (valueAsValueType == null)
            {
                throw new LuaException("Only value type keys allowed in a managed object table.");
            }

            return valueAsValueType;
        }

        private void RemoveLuaReferenceOfAllSubTables(Lua lua)
        {
            var subTables = this.CopySubTableFieldsDictionary();

            foreach (var item in subTables)
            {
                item.Value.RemoveLuaReference(lua);
            }
        }

        private void ClearTableFields()
        {
            lock (this.tableFields)
            {
                this.tableFields.Clear();
            }
        }

        private int GetTableFieldCount()
        {
            lock (this.tableFields)
            {
                return this.tableFields.Count;
            }
        }

        private LuaManagedObjectTableField GetTableFieldIfKeyExists(LuaValueType key)
        {
            lock (this.tableFields)
            {
                if (!this.tableFields.ContainsKey(key))
                {
                    return null;
                }

                return this.tableFields[key];
            }
        }

        private IDictionary<LuaValueType, LuaManagedObjectTableField> CopyFieldsDictionary()
        {
            Dictionary<LuaValueType, LuaManagedObjectTableField> dictionary;

            lock (this.tableFields)
            {
                dictionary = new Dictionary<LuaValueType, LuaManagedObjectTableField>(this.tableFields);
            }

            return dictionary;
        }

        private IDictionary<LuaValueType, TSubTable> CopySubTableFieldsDictionary()
        {
            var dictionary = new Dictionary<LuaValueType, TSubTable>();

            lock (this.tableFields)
            {
                foreach (var item in this.tableFields)
                {
                    var subTableField = item.Value as LuaManagedObjectSubTableField<TSubTable>;

                    if (subTableField != null)
                    {
                        dictionary.Add(item.Key, subTableField.Value);
                    }
                }
            }

            return dictionary;
        }

        private void SetTableField(LuaValueType key, LuaManagedObjectTableField value)
        {
            lock (this.tableFields)
            {
                this.tableFields[key] = value;
            }
        }
    }
}
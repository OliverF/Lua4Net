namespace Lua4Net
{
    public class LuaManagedObjectSubTableField<T> : LuaManagedObjectTableField
        where T : LuaManagedObjectTable<T>, new()
    {
        public LuaManagedObjectSubTableField(T subTable)
        {
            this.Value = subTable;
        }

        public T Value { get; private set; }
    }
}
namespace Proj_Biblioteca.Data
{
    public interface IDAO
    {
        //Interfaccia per i metodi CRUD 

        public Task<List<Entity>> ReadAll();

        public Task<bool> Delete(int id);

        public Task<bool> Insert(Entity e);

        public Task<bool> Update(Entity e);

        public Task<Entity> Find(int id);
    }
}

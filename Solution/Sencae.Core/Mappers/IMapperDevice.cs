namespace Sencae.Mappers;

public interface IMapperDevice : IBusDevice, IDisposable {
    virtual void Connect(Machine machine) {
    }
    
    virtual void Reset() {
    }
    
    void IDisposable.Dispose() {
        throw new NotImplementedException();
    }
}

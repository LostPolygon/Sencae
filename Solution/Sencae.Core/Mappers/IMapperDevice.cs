using System;

namespace Sencae.Mappers;

public interface IMapperDevice : IBusDevice, IDisposable {
    virtual void Reset() {
    }
    
    void IDisposable.Dispose() {
        throw new NotImplementedException();
    }
}

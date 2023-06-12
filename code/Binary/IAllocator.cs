namespace Mikodev.Binary;

public interface IAllocator
{
    ref byte Resize(int length);
}

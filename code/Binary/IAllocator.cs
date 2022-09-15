namespace Mikodev.Binary;

public interface IAllocator
{
    ref byte Allocate(int required);
}

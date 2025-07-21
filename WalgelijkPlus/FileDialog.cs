using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeFileDialogExtendedSharp;
using Walgelijk;

namespace WalgelijkPlus;

public static class FileDialog
{
    public static bool OpenFile((string Description, string Specification)[]? filters, out string path)
    {
        path = string.Empty;

        var r = Nfd.FileOpen(filters == null ? [] : filters.Select(static v => new NfdFilter
        {
            Description = v.Description,
            Specification = v.Specification
        }));

        switch (r.Status)
        {
            case NfdStatus.Error:
#if DEBUG
                throw new Exception(r.Error);
#else
                Logger.Error(r.Error);
                return false;
#endif
            case NfdStatus.Ok:
                path = r.Path;
                return true;
        }

        return false;
    }

    public static bool SaveFile((string Description, string Specification)[]? filters, string? defaultName, string? defaultPath, out string path)
    {
        path = string.Empty;

        var r = Nfd.FileSave(filters == null ? [] : filters.Select(static v => new NfdFilter
        {
            Description = v.Description,
            Specification = v.Specification
        }), defaultName, defaultPath);

        switch (r.Status)
        {
            case NfdStatus.Error:
#if DEBUG
                throw new Exception(r.Error);
#else
                Logger.Error(r.Error);
                return false;
#endif
            case NfdStatus.Ok:
                path = r.Path;
                return true;
        }

        return false;
    }
}

﻿using System;
using System.IO;
using System.Text;

// borrowed the idea from http://damieng.com/blog/2008/07/30/linq-to-sql-log-to-debug-window-file-memory-or-multiple-writers
public abstract class LineWriter : TextWriter
{
    private readonly StringBuilder _Buffer = new StringBuilder();
    private bool _CRFound = false;

    public bool IsDisposing { get; private set; } = false;
    public bool IsDisposed { get; private set; } = false;

    protected LineWriter(IFormatProvider formatProvider)
        : base(formatProvider)
    { }

    protected virtual string DecorateLine(string source) => source;
    protected abstract void FlushLine(string line);

    private void FlushLine()
    {
        FlushLine(DecorateLine(_Buffer.ToString()));
        _Buffer.Clear();
    }

    public override void Write(char value)
    {
        switch (value)
        {
            case '\n':
                if (_CRFound)
                {
                    _CRFound = false;
                    return;
                }
                break;
            case '\r':
                _CRFound = true;
                break;
            default:
                _CRFound = false;
                _Buffer.Append(value);
                return;
        }

        FlushLine();
    }

    protected override void Dispose(bool disposing)
    {
        if (!IsDisposing)
        {
            IsDisposing = true;
            try
            {
                if (_Buffer.Length > 0)
                    FlushLine();
            }
            finally
            {
                IsDisposed = true;
            }
        }

        base.Dispose(disposing);
    }

    public override Encoding Encoding => Defaults.UnicodeEncoding;
}

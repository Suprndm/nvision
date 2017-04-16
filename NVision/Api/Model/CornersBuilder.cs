using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVision.Internal.Model;

namespace NVision.Api.Model
{
    public static class CornersBuilder
    {
        private const int _cornerSize = 30;

        public static Form BuildTopLeftCornerForm()
        {

            var mask = new bool[_cornerSize, _cornerSize];
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i >= _cornerSize / 2 && j >= _cornerSize / 2)
                    {
                        if (i == _cornerSize / 2 || j == _cornerSize / 2)
                        {
                            mask[i, j] = true;
                        }
                    }
                }
            }

            var form = new Form(mask, _cornerSize, FormType.TopLeft);

            return form;
        }

        public static Form BuildTopRightCornerForm()
        {
            var mask = new bool[_cornerSize, _cornerSize];

            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i <= _cornerSize / 2 && j >= _cornerSize / 2)
                    {
                        if (i == _cornerSize / 2 || j == _cornerSize / 2)
                        {
                            mask[i, j] = true;
                        }
                    }
                }
            }

            var form = new Form(mask, _cornerSize, FormType.TopRight);

            return form;
        }

        public static Form BuildBottomLeftCornerForm()
        {
            var mask = new bool[_cornerSize, _cornerSize];
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i >= _cornerSize / 2 && j <= _cornerSize / 2)
                    {
                        if (i == _cornerSize / 2 || j == _cornerSize / 2)
                        {
                            mask[i, j] = true;
                        }
                    }
                }
            }
            var form = new Form(mask, _cornerSize, FormType.BottomLeft);

            return form;
        }

        public static Form BuildBottomRightCornerForm()
        {
            var mask = new bool[_cornerSize, _cornerSize];
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i <= _cornerSize / 2 && j <= _cornerSize / 2)
                    {
                        if (i == _cornerSize / 2 || j == _cornerSize / 2)
                        {
                            mask[i, j] = true;
                        }
                    }
                }
            }

            var form = new Form(mask, _cornerSize, FormType.BottomRight);

            return form;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVision.Api.Model
{
    public class CornersBuilder
    {
        private const int _cornerSize = 30;

        public Form BuildTopLeftCornerForm()
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

            var form = new Form(mask, _cornerSize, "topLeftCorner");

            return form;
        }

        public Form BuildTopRightCornerForm()
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

            var form = new Form(mask, _cornerSize, "TopRightCorner");

            return form;
        }

        public Form BuildBottomLeftCornerForm()
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
            var form = new Form(mask, _cornerSize, "bottomLeftCorner");

            return form;
        }

        public Form BuildBottomRightCornerForm()
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

            var form = new Form(mask, _cornerSize, "bottomRightCorner");

            return form;
        }

    }
}

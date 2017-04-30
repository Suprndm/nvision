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

        public static IList<Form> GetCornerForms()
        {

            // 0
            var forms = new List<Form>();
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
            forms.Add(form);

            // 45
            mask = new bool[_cornerSize, _cornerSize];
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i >= _cornerSize / 2)
                    {
                        if (i == j || i == _cornerSize - j)
                        {
                            mask[i, j] = true;
                        }
                    }
                }
            }

            forms.Add(new Form(mask, _cornerSize, FormType.TopLeft));
            forms.Add(new Form(mask, _cornerSize, FormType.BottomLeft));


            //315
            mask = new bool[_cornerSize, _cornerSize];
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (j >= _cornerSize / 2)
                    {
                        if (i == j || i == _cornerSize - j)
                        {
                            mask[i, j] = true;
                        }
                    }
                }
            }

            forms.Add(new Form(mask, _cornerSize, FormType.TopLeft));
            forms.Add(new Form(mask, _cornerSize, FormType.TopRight));



            //270
            mask = new bool[_cornerSize, _cornerSize];
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

            forms.Add(new Form(mask, _cornerSize, FormType.TopRight));


            //225
            mask = new bool[_cornerSize, _cornerSize];
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i <= _cornerSize / 2)
                    {
                        if (i == j || i == _cornerSize - j)
                        {
                            mask[i, j] = true;
                        }
                    }
                }
            }

            forms.Add(new Form(mask, _cornerSize, FormType.TopRight));
            forms.Add(new Form(mask, _cornerSize, FormType.BottomRight));


            // 180
            mask = new bool[_cornerSize, _cornerSize];
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

            forms.Add(new Form(mask, _cornerSize, FormType.BottomRight));

            // 135
            mask = new bool[_cornerSize, _cornerSize];
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (j <= _cornerSize / 2)
                    {
                        if (i == j || i == _cornerSize - j)
                        {
                            mask[i, j] = true;
                        }
                    }
                }
            }

            forms.Add(new Form(mask, _cornerSize, FormType.BottomRight));
            forms.Add(new Form(mask, _cornerSize, FormType.BottomLeft));

            // 90
            mask = new bool[_cornerSize, _cornerSize];
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
            forms.Add(new Form(mask, _cornerSize, FormType.BottomLeft));

            return forms;
        }

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

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
        private const double _fuzzinessSpread = 0.3;
        private const double _fuzzinessStrenght = 1;


        public Form BuildTopLeftCornerForm()
        {
          
            var form = new Form(_cornerSize);
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i >= _cornerSize / 2 && j >= _cornerSize / 2)
                    {
                        if (i == _cornerSize / 2 || j == _cornerSize / 2)
                        {
                            form.Mask[i, j] = 1;
                            form.WhitePixelCount++;
                        }
                    }
                }
            }

            form = Fuzzify(form);

            return form;
        }

        public Form BuildTopRightCornerForm()
        {

            var form = new Form(_cornerSize);
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i <= _cornerSize / 2 && j >= _cornerSize / 2)
                    {
                        if (i == _cornerSize / 2 || j == _cornerSize / 2)
                        {
                            form.Mask[i, j] = 1;
                            form.WhitePixelCount++;
                        }
                    }
                }
            }

            form = Fuzzify(form);

            return form;
        }

        public Form BuildBottomLeftCornerForm()
        {

            var form = new Form(_cornerSize);
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i >= _cornerSize / 2 && j <= _cornerSize / 2)
                    {
                        if (i == _cornerSize / 2 || j == _cornerSize / 2)
                        {
                            form.Mask[i, j] = 1;
                            form.WhitePixelCount++;
                        }
                    }
                }
            }

            form = Fuzzify(form);

            return form;
        }

        public Form BuildBottomRightCornerForm()
        {

            var form = new Form(_cornerSize);
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (i <= _cornerSize / 2 && j <= _cornerSize / 2)
                    {
                        if (i == _cornerSize / 2 || j == _cornerSize / 2)
                        {
                            form.Mask[i, j] = 1;
                            form.WhitePixelCount++;
                        }
                    }
                }
            }

            form = Fuzzify(form);

            return form;
        }

        private Form Fuzzify(Form form)
        {
            var fuzinessSize = (int)(_cornerSize * _fuzzinessSpread);

            // Fuzzifie
            for (int i = 0; i < _cornerSize; i++)
            {
                for (int j = 0; j < _cornerSize; j++)
                {
                    if (form.Mask[i, j] == 1)
                    {
                        for (int x = 0; x < fuzinessSize; x++)
                        {
                            for (int y = 0; y < fuzinessSize; y++)
                            {
                                var posX = i - (x - fuzinessSize / 2);
                                var posY = j - (y - fuzinessSize / 2);
                                if (posX < _cornerSize && posX > 0 && posY < _cornerSize && posY > 0 && posX != i && posY != j && form.Mask[posX, posY] != 1)
                                {
                                    var distanceX = (double)(1 + fuzinessSize / 2 - Math.Abs(x - fuzinessSize / 2)) / fuzinessSize / 2;
                                    var distanceY = (double)(1 + fuzinessSize / 2 - Math.Abs(y - fuzinessSize / 2)) / fuzinessSize / 2;
                                    form.Mask[posX, posY] += _fuzzinessStrenght * ((Math.Min(distanceX, distanceY)) / 2);
                                }
                            }
                        }
                    }
                }
            }

            return form;
        }
    }
}

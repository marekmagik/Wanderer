﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    public class ObservablePlacesCollection : ObservableCollection<ImageMetadata>
    {

        public void Sort()
        {
            List<ImageMetadata> sorted = this.OrderBy(x => x).ToList();

            for (int i = 0; i < sorted.Count; i++) {
                if (!this[i].Equals(sorted[i])) {
                    ImageMetadata place = sorted.ElementAt(i);
                    this.Remove(place);
                    this.Insert(sorted.IndexOf(place), place);
                }
            }
        }
    }
}

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TripsGrid } from './trips-grid';

describe('TripsGrid', () => {
  let component: TripsGrid;
  let fixture: ComponentFixture<TripsGrid>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TripsGrid]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TripsGrid);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

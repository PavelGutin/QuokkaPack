/* Manual alias file for OpenAPI-generated types.
   Update if your OpenAPI schema names change. */

import type { components } from '../../../app/api/types.gen';

// Domain & DTOs
export type AppUserLogin          = components['schemas']['AppUserLogin'];
export type Category              = components['schemas']['Category'];
export type CategoryCreateDto     = components['schemas']['CategoryCreateDto'];
export type CategoryEditDto       = components['schemas']['CategoryEditDto'];
export type CategoryReadDto       = components['schemas']['CategoryReadDto'];
export type CategoryReadDtoSimple = components['schemas']['CategoryReadDtoSimple'];

export type Item                  = components['schemas']['Item'];
export type ItemCreateDto         = components['schemas']['ItemCreateDto'];
export type ItemEditDto           = components['schemas']['ItemEditDto'];
export type ItemReadDto           = components['schemas']['ItemReadDto'];

export type LoginModel            = components['schemas']['LoginModel'];
export type MasterUser            = components['schemas']['MasterUser'];
export type RegisterRequest       = components['schemas']['RegisterRequest'];
export type SetupRequest          = components['schemas']['SetupRequest'];

export type Trip                  = components['schemas']['Trip'];
export type TripCreateDto         = components['schemas']['TripCreateDto'];
export type TripEditDto           = components['schemas']['TripEditDto'];
export type TripReadDto           = components['schemas']['TripReadDto'];

export type TripItem              = components['schemas']['TripItem'];
export type TripItemCreateDto     = components['schemas']['TripItemCreateDto'];
export type TripItemEditDto       = components['schemas']['TripItemEditDto'];
export type TripItemReadDto       = components['schemas']['TripItemReadDto'];

export type WeatherForecast       = components['schemas']['WeatherForecast'];

/* Example: UI-only view model extensions can live here too.
export type ItemVM = ItemReadDto & { isEditing?: boolean };
*/

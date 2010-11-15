module ComponentInterfaceModule
   implicit none

   ! get_simulation_information kind parameters.
   integer, parameter :: respondToGetInfo = 2
   integer, parameter :: respondToSetInfo = 3
   integer, parameter :: respondToGetSetInfo = 4
   integer, parameter :: eventInfo = 5
   integer, parameter :: respondToEventInfo = 6
   integer, parameter :: componentInfo = 8

   integer, parameter :: getVariableReg = 1
   integer, parameter :: respondToGetReg = 2
   integer, parameter :: setVariableReg = 9
   integer, parameter :: respondToSetReg = 3
   integer, parameter :: respondToGetSetReg = 4
   integer, parameter :: eventReg = 5
   integer, parameter :: respondToEventReg=6

   interface
      function add_registration(kind, name, typeString, alias)
      ml_external add_registration
      integer, intent(in)           :: kind
      character (len=*), intent(in) :: name
      character (len=*), intent(in) :: typeString
      character (len=*), intent(in) :: alias
      integer                       :: add_registration
      end function add_registration

      function add_registration_with_units(kind, name, typeString, units)
      ml_external add_registration_with_units
      integer, intent(in)           :: kind
      character (len=*), intent(in) :: name
      character (len=*), intent(in) :: typeString
      character (len=*), intent(in) :: units
      integer                       :: add_registration_with_units
      end function add_registration_with_units

      function add_reg(kind, name, typeString, units, desc)
      ml_external add_reg
      integer, intent(in)           :: kind
      character (len=*), intent(in) :: name
      character (len=*), intent(in) :: typeString
      character (len=*), intent(in) :: units
      character (len=*), intent(in) :: desc
      integer                       :: add_reg
      end function add_reg

      subroutine fortran_error(msg, isFatal)
      ml_external fortran_error
      character (len=*), intent(in) :: msg
      logical          , intent(in) :: isFatal
      end subroutine fortran_error

      subroutine terminate_simulation()
      ml_external terminate_simulation
      end subroutine terminate_simulation

      subroutine message_unused()
      ml_external message_unused
      end subroutine message_unused

      subroutine get_name(name)
      ml_external get_name
      character (len=*), intent(in out) :: name
      end subroutine get_name

!      function get_variable_internal(registrationID, value, optionalVar)
!      ml_external get_variable_internal
!      integer, intent(in)  :: registrationID
!      integer, intent(out) :: value
!      logical, intent(in)  :: optionalVar
!      logical              :: get_variable_internal
!      end function get_variable_internal
!
!      function get_variables(registrationID, values)
!      ml_external get_variables
!      integer, intent(in)  :: registrationID
!      integer, intent(out) :: values
!      logical              :: get_variables
!      end function get_variables
!
      subroutine write_string(st)
      ml_external write_string
      character (len=*), intent(in) :: st
      end subroutine write_string

      subroutine writeStdErr(st)
      ml_external writeStdErr
      character (len=*), intent(in) :: st
      end subroutine writeStdErr

      function read_parameter(parameterName, sectionName, value, isOptional)
      ml_external read_parameter
      character(len=*), intent(in) :: parameterName
      character(len=*), intent(in) :: sectionName
      character(len=*), intent(in out) :: value
      logical                          :: isOptional
      logical                          :: read_parameter
      end function

      subroutine getApsuiteDirectory(dir)
      ml_external getApsuiteDirectory
      character(len=*),intent(in out) :: dir
      end subroutine getApsuiteDirectory

      function get_componentData( )
      ml_external get_componentData
      integer :: get_componentData
      end function

      function get_componentID( )
      ml_external get_componentID
      integer :: get_componentID
      end function

      subroutine apsimcomponentdata_getrulenames(componentData,      &
                                                 ruleNames,          &
                                                 maxNumRules,        &
                                                 numRules)
      ml_external ApsimComponentData_getRuleNames
      integer, intent(in)          :: componentData
      character(len=*), intent(in) :: ruleNames(*)
      integer, intent(in)          :: maxNumRules
      integer, intent(in)          :: numRules
      end subroutine

      subroutine apsimcomponentdata_loadrule(componentData,          &
                                             ruleNames)
      ml_external ApsimComponentData_loadRule
      integer, intent(in)          :: componentData
      character(len=*), intent(in) :: ruleNames(*)
      end subroutine

      function ApsimComponentData_getNumRuleLines( )
      ml_external ApsimComponentData_getNumRuleLines
      integer :: apsimcomponentdata_getnumrulelines
      end function

      subroutine ApsimComponentData_getRuleLine(lineNumber, Line)
      ml_external ApsimComponentData_getRuleLine
      integer, intent(in) :: lineNumber
      character(len=*), intent(in out) :: line
      end subroutine

      subroutine ApsimComponentData_getRuleCondition(condition)
      ml_external ApsimComponentData_getRuleCondition
      character(len=*), intent(in out) :: condition
      end subroutine

      function newApsimDataFile(fileName)
      ml_external newApsimDataFile
      character(len=*), intent(in) :: fileName
      integer                      :: newApsimDataFile
      end function newApsimDataFile

      subroutine deleteApsimDataFile(dataFile)
      ml_external deleteApsimDataFile
      integer, intent(in)          :: dataFile
      end subroutine deleteApsimDataFile

      function ApsimDataFile_getFieldValue(dataFile, fieldIndex, value)
      ml_external ApsimDataFile_getFieldValue
      integer, intent(in)              :: dataFile
      integer, intent(in)              :: fieldIndex
      character(len=*), intent(in out) :: value
      logical                          :: ApsimDataFile_getFieldValue
      end function ApsimDataFile_getFieldValue

      function ApsimDataFile_next(dataFile)
      ml_external ApsimDataFile_next
      integer, intent(in)          :: dataFile
      logical                      :: ApsimDataFile_next
      end function ApsimDataFile_next


!      subroutine send_message(amessage)
!      use ProtocolModule
!      ml_external send_message
!      type(Message), intent(in) :: amessage
!      end subroutine send_message
!
!

       subroutine new_postbox( )
       ml_external new_postbox
       end subroutine new_postbox

       subroutine delete_postbox( )
       ml_external delete_postbox
       end subroutine delete_postbox

       function store_in_postbox(str)
       ml_external store_in_postbox
       character(len=*), intent(in) :: str
       logical*1 :: store_in_postbox
       end function store_in_postbox

       function get_posting_module()
       ml_external get_posting_module
       integer :: get_posting_module
       end function

       function component_id_to_name(id, name)
       ml_external component_id_to_name
       integer, intent(in)              :: id
       character(len=*), intent(in out) :: name
       logical*1                         :: component_id_to_name
       end function component_id_to_name

       function component_name_to_id(name, id)
       ml_external component_name_to_id
       character(len=*), intent(in) :: name
       integer, intent(out)         :: id
       logical*1                      :: component_name_to_id
       end function component_name_to_id

       subroutine event_send(moduleID, eventName)
       ml_external event_send
       integer, intent(in)          :: moduleID
       character(len=*), intent(in) :: eventName
       end subroutine event_send

       subroutine get_integer_var(componentID, variableName, units, value, numvals, lower, upper)
       ml_external get_integer_var
       integer, intent(in)          :: componentID
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       integer, intent(in out)         :: value
       integer, intent(in out)      :: numvals
       integer, intent(in)             :: lower
       integer, intent(in)             :: upper
       end subroutine get_integer_var

       subroutine get_integer_vars(requestNo, variableName, units, value, numvals, lower, upper)
       ml_external get_integer_vars
       integer, intent(in)          :: requestNo
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       integer, intent(in out)      :: value
       integer, intent(in out)      :: numvals
       integer, intent(in)          :: lower
       integer, intent(in)          :: upper
       end subroutine get_integer_vars

       subroutine get_real_var(componentID, variableName, units, value, numvals, lower, upper)
       ml_external get_real_var
       integer, intent(in)          :: componentID
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       real, intent(in out)         :: value
       integer, intent(in out)      :: numvals
       real, intent(in)             :: lower
       real, intent(in)             :: upper
       end subroutine get_real_var

       subroutine get_real_vars(requestNo, variableName, units, value, numvals, lower, upper)
       ml_external get_real_vars
       integer, intent(in)          :: requestNo
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       real, intent(in out)         :: value
       integer, intent(in out)      :: numvals
       real, intent(in)             :: lower
       real, intent(in)             :: upper
       end subroutine get_real_vars

       subroutine get_real_array(componentID, variableName, arraySize, units, value, numvals, lower, upper)
       ml_external get_real_array
       integer, intent(in)          :: componentID
       character(len=*), intent(in) :: variableName
       integer, intent(in)          :: arraySize
       character(len=*), intent(in) :: units
       real, intent(in out)         :: value(*)
       integer, intent(in out)      :: numvals
       real, intent(in)             :: lower
       real, intent(in)             :: upper
       end subroutine get_real_array

       subroutine get_real_arrays(requestNo, variableName, arraySize, units, value, numvals, lower, upper)
       ml_external get_real_arrays
       integer, intent(in)          :: requestNo
       character(len=*), intent(in) :: variableName
       integer, intent(in)          :: arraySize
       character(len=*), intent(in) :: units
       real, intent(in out)         :: value(*)
       integer, intent(in out)      :: numvals
       real, intent(in)             :: lower
       real, intent(in)             :: upper
       end subroutine get_real_arrays

       subroutine get_real_array_optional(componentID, variableName, arraySize, units, value, numvals, lower, upper)
       ml_external get_real_array_optional
       integer, intent(in)          :: componentID
       character(len=*), intent(in) :: variableName
       integer, intent(in)          :: arraySize
       character(len=*), intent(in) :: units
       real, intent(in out)         :: value(*)
       integer, intent(in out)      :: numvals
       real, intent(in)             :: lower
       real, intent(in)             :: upper
       end subroutine get_real_array_optional

       subroutine get_real_var_optional(componentID, variableName, units, value, numvals, lower, upper)
       ml_external get_real_var_optional
       integer, intent(in)          :: componentID
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       real, intent(in out)         :: value
       integer, intent(in out)      :: numvals
       real, intent(in)             :: lower
       real, intent(in)             :: upper
       end subroutine get_real_var_optional

       subroutine get_double_var(componentID, variableName, units, value, numvals, lower, upper)
       ml_external get_double_var
       integer, intent(in)              :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       double precision, intent(in out) :: value
       integer, intent(in out)          :: numvals
       double precision, intent(in)     :: lower
       double precision, intent(in)     :: upper
       end subroutine get_double_var

       subroutine get_double_var_optional(componentID, variableName, units, value, numvals, lower, upper)
       ml_external get_double_var_optional
       integer, intent(in)          :: componentID
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       double precision, intent(in out) :: value
       integer, intent(in out)          :: numvals
       double precision, intent(in)     :: lower
       double precision, intent(in)     :: upper
       end subroutine get_double_var_optional

       subroutine get_double_vars(requestNo, variableName, units, value, numvals, lower, upper)
       ml_external get_double_vars
       integer, intent(in)          :: requestNo
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       double precision, intent(in out) :: value
       integer, intent(in out)          :: numvals
       double precision, intent(in)     :: lower
       double precision, intent(in)     :: upper
       end subroutine get_double_vars

       subroutine get_double_array(componentID, variableName, arraySize, units, value, numvals, lower, upper)
       ml_external get_double_array
       integer, intent(in)          :: componentID
       character(len=*), intent(in) :: variableName
       integer, intent(in)          :: arraySize
       character(len=*), intent(in) :: units
       double precision, intent(in out) :: value(*)
       integer, intent(in out)          :: numvals
       double precision, intent(in)     :: lower
       double precision, intent(in)     :: upper
       end subroutine get_double_array

       subroutine get_char_var(componentID, variableName, units, value, numvals)
       ml_external get_char_var
       integer, intent(in)              :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       character(len=*), intent(in out) :: value
       integer, intent(in out)          :: numvals
       end subroutine get_char_var

       subroutine get_char_var_optional(componentID, variableName, units, value, numvals)
       ml_external get_char_var_optional
       integer, intent(in)          :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       character(len=*), intent(in out) :: value
       integer, intent(in out)          :: numvals
       end subroutine get_char_var_optional

       subroutine get_char_vars(requestNo, variableName, units, value, numvals)
       ml_external get_char_vars
       integer, intent(in)          :: requestNo
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       character(len=*), intent(in out) :: value
       integer, intent(in out)      :: numvals
       end subroutine get_char_vars

       subroutine set_real_var(componentID, variableName, units, value)
       ml_external set_real_var
       integer, intent(in)              :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       real, intent(in)                 :: value
       end subroutine set_real_var

       subroutine set_real_array(componentID, variableName, units, value, numvals)
       ml_external set_real_array
       integer, intent(in)              :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       real, intent(in)                 :: value(*)
       integer, intent(in)              :: numvals
       end subroutine set_real_array

       subroutine set_double_var(componentID, variableName, units, value)
       ml_external set_double_var
       integer, intent(in)              :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       double precision, intent(in)     :: value
       end subroutine set_double_var

       subroutine set_double_array(componentID, variableName, units, value, numvals)
       ml_external set_double_array
       integer, intent(in)              :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       double precision, intent(in)     :: value(*)
       integer, intent(in)              :: numvals
       end subroutine set_double_array

       subroutine set_char_var(componentID, variableName, units, value)
       ml_external set_char_var
       integer, intent(in)              :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       character(len=*), intent(in)     :: value
       end subroutine set_char_var

       subroutine set_char_var_optional(componentID, variableName, units, numvals, value)
       ml_external set_char_var_optional
       integer, intent(in)              :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       integer, intent(out)             :: numvals
       character(len=*), intent(in)     :: value
       end subroutine set_char_var_optional

       subroutine set_char_array(componentID, variableName, units, value, numvals)
       ml_external set_char_array
       integer, intent(in)              :: componentID
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       character(len=*), intent(in)     :: value(*)
       integer, intent(in)              :: numvals
       end subroutine set_char_array

       subroutine respond2get_integer_var(variableName, units, value)
       ml_external respond2get_integer_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       integer, intent(in) :: value
       end subroutine respond2get_integer_var

       subroutine respond2get_integer_array(variableName, units, value, numvals)
       ml_external respond2get_integer_array
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       integer, intent(in) :: value(*)
       integer, intent(in) :: numvals
       end subroutine respond2get_integer_array

       subroutine respond2get_real_var(variableName, units, value)
       ml_external respond2get_real_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       real, intent(in) :: value
       end subroutine respond2get_real_var

       subroutine respond2get_real_array(variableName, units, value, numvals)
       ml_external respond2get_real_array
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       real, intent(in) :: value(*)
       integer, intent(in) :: numvals
       end subroutine respond2get_real_array

       subroutine respond2get_double_var(variableName, units, value)
       ml_external respond2get_double_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       double precision, intent(in) :: value
       end subroutine respond2get_double_var

       subroutine respond2get_double_array(variableName, units, value, numvals)
       ml_external respond2get_double_array
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       double precision, intent(in) :: value(*)
       integer, intent(in) :: numvals
       end subroutine respond2get_double_array

       subroutine respond2get_logical_var(variableName, units, value)
       ml_external respond2get_logical_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       logical, intent(in) :: value
       end subroutine respond2get_logical_var

       subroutine respond2get_char_var(variableName, units, value)
       ml_external respond2get_char_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       character(len=*), intent(in) :: value
       end subroutine respond2get_char_var

       subroutine respond2get_char_array(variableName, units, value, numvals)
       ml_external respond2get_char_array
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       character(len=*), intent(in) :: value(*)
       integer, intent(in) :: numvals
       end subroutine respond2get_char_array

       subroutine respond2get_time_var(variableName, units, value)
       use dataTypes
       ml_external respond2get_time_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       type(timeType), intent(in) :: value
       end subroutine respond2get_time_var

       subroutine post_integer_var(variableName, units, value)
       ml_external post_integer_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       integer, intent(in) :: value
       end subroutine post_integer_var

       subroutine post_real_var(variableName, units, value)
       ml_external post_real_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       real, intent(in) :: value
       end subroutine post_real_var

       subroutine post_real_array(variableName, units, value, numvals)
       ml_external post_real_array
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       real, intent(in) :: value(*)
       integer, intent(in) :: numvals
       end subroutine post_real_array

       subroutine post_double_var(variableName, units, value)
       ml_external post_double_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       double precision, intent(in) :: value
       end subroutine post_double_var

       subroutine post_double_array(variableName, units, value, numvals)
       ml_external post_double_array
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       double precision, intent(in) :: value(*)
       integer, intent(in) :: numvals
       end subroutine post_double_array

       subroutine post_char_var(variableName, units, value)
       ml_external post_char_var
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       character(len=*), intent(in) :: value
       end subroutine post_char_var

       subroutine post_char_array(variableName, units, value, numvals)
       ml_external post_char_array
       character(len=*), intent(in) :: variableName
       character(len=*), intent(in) :: units
       character(len=*), intent(in) :: value(*)
       integer, intent(in)          :: numvals
       end subroutine post_char_array

       subroutine collect_char_var(variableName, units, value, numvals)
       ml_external collect_char_var
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       character(len=*), intent(in out) :: value
       integer, intent(in out)          :: numvals
       end subroutine collect_char_var

       subroutine collect_char_var_optional(variableName, units, value, numvals)
       ml_external collect_char_var_optional
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       character(len=*), intent(in out) :: value
       integer, intent(in out)          :: numvals
       end subroutine collect_char_var_optional

       subroutine collect_char_array(variableName, arraySize, units, value, numvals)
       ml_external collect_char_array
       character(len=*), intent(in)     :: variableName
       integer, intent(in)              :: arraySize
       character(len=*), intent(in)     :: units
       character(len=*), intent(in out) :: value(*)
       integer, intent(in out)          :: numvals
       end subroutine collect_char_array

       subroutine collect_char_array_optional(variableName, arraySize, units, value, numvals)
       ml_external collect_char_array_optional
       character(len=*), intent(in)     :: variableName
       integer, intent(in)              :: arraySize
       character(len=*), intent(in)     :: units
       character(len=*), intent(in out) :: value(*)
       integer, intent(in out)          :: numvals
       end subroutine collect_char_array_optional

       subroutine collect_real_var(variableName, units, value, numvals, lower, upper)
       ml_external collect_real_var
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       real, intent(in out)             :: value
       integer, intent(in out)          :: numvals
       real, intent(in)                 :: lower
       real, intent(in)                 :: upper
       end subroutine collect_real_var

       subroutine collect_real_var_optional(variableName, units, value, numvals, lower, upper)
       ml_external collect_real_var_optional
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       real, intent(in out)             :: value
       integer, intent(in out)          :: numvals
       real, intent(in)                 :: lower
       real, intent(in)                 :: upper
       end subroutine collect_real_var_optional

       subroutine collect_real_array(variableName, arraySize, units, value, numvals, lower, upper)
       ml_external collect_real_array
       character(len=*), intent(in)     :: variableName
       integer, intent(in)              :: arraySize
       character(len=*), intent(in)     :: units
       real, intent(in out)             :: value(*)
       integer, intent(in out)          :: numvals
       real, intent(in)                 :: lower
       real, intent(in)                 :: upper
       end subroutine collect_real_array

       subroutine collect_real_array_optional(variableName, arraySize, units, value, numvals, lower, upper)
       ml_external collect_real_array_optional
       character(len=*), intent(in)     :: variableName
       integer, intent(in)              :: arraySize
       character(len=*), intent(in)     :: units
       real, intent(in out)             :: value(*)
       integer, intent(in out)          :: numvals
       real, intent(in)                 :: lower
       real, intent(in)                 :: upper
       end subroutine collect_real_array_optional

       subroutine collect_integer_var(variableName, units, value, numvals, lower, upper)
       ml_external collect_integer_var
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       integer, intent(in out)          :: value
       integer, intent(in out)          :: numvals
       integer, intent(in)              :: lower
       integer, intent(in)              :: upper
       end subroutine collect_integer_var

       subroutine collect_integer_var_optional(variableName, units, value, numvals, lower, upper)
       ml_external collect_integer_var_optional
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       integer, intent(in out)          :: value
       integer, intent(in out)          :: numvals
       integer, intent(in)              :: lower
       integer, intent(in)              :: upper
       end subroutine collect_integer_var_optional

       subroutine collect_double_var(variableName, units, value, numvals, lower, upper)
       ml_external collect_double_var
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       double precision, intent(in out) :: value
       integer, intent(in out)          :: numvals
       double precision, intent(in)     :: lower
       double precision, intent(in)     :: upper
       end subroutine collect_double_var

       subroutine collect_double_var_optional(variableName, units, value, numvals, lower, upper)
       ml_external collect_double_var_optional
       character(len=*), intent(in)     :: variableName
       character(len=*), intent(in)     :: units
       double precision, intent(in out) :: value
       integer, intent(in out)          :: numvals
       double precision, intent(in)     :: lower
       double precision, intent(in)     :: upper
       end subroutine collect_double_var_optional

       subroutine collect_double_array(variableName, arraySize, units, value, numvals, lower, upper)
       ml_external collect_double_array
       character(len=*), intent(in)     :: variableName
       integer, intent(in)              :: arraySize
       character(len=*), intent(in)     :: units
       double precision, intent(in out) :: value(*)
       integer, intent(in out)          :: numvals
       double precision, intent(in)     :: lower
       double precision, intent(in)     :: upper
       end subroutine collect_double_array

       subroutine change_component_order(module_list, numvals)
       ml_external change_component_order
       character(len=*), intent(in)     :: module_list(*)
       integer, intent(in)              :: numvals
       end subroutine change_component_order

       subroutine get_fq_name(name)
       ml_external get_fq_name
       character(len=*), intent(in out) :: name
       end subroutine get_fq_name
	   
       function string_to_float(inString, isOk)
       ml_external string_to_float
       character (len=*), intent(in) :: inString
       logical*1, intent(out)        :: isOk
       double precision string_to_float
       end function string_to_float	   

   end interface

end module ComponentInterfaceModule

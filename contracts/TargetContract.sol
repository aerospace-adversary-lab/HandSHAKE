// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

import "./node_modules/@openzeppelin/contracts/access/AccessControl.sol";

contract TargetContract is AccessControl {
    struct Message {
        uint8 targetType; // Type of the target (0 for "ground" or 1 for "orbital")
        int256[2] groundCoordinates; // Coordinates for ground targets
        uint256 tleLine1; // Integer for TLE data for orbital targets
        uint256 tleLine2; // Second line of TLE data for orbital targets
        uint256[2] timeWindow; // Start and end time
        address requesterId; // Address of the requester
        address proposerId; // Address of the proposer
        bytes data; // Additional data
        bool requesterValidation; // Validation status
    }

    bytes32 public constant PROPOSER_ROLE = keccak256("PROPOSER_ROLE");
    bytes32 public constant REQUESTER_ROLE = keccak256("REQUESTER_ROLE");

    Message public message;
    bool public isLocked;
    bool public isValidationLocked;
    bool public isMessageAdded;

    event MessageAdded(uint8 targetType, address requesterId);
    event MessageLocked();
    event ValidationLocked();

    constructor(address proposer, address requester) {
        require(
            proposer == msg.sender,
            "Proposer must be the contract deployer"
        );

        // Grant the deployer (msg.sender) the proposer role
        _grantRole(DEFAULT_ADMIN_ROLE, proposer);
        _grantRole(PROPOSER_ROLE, proposer);
        _grantRole(REQUESTER_ROLE, requester);
        _setRoleAdmin(REQUESTER_ROLE, PROPOSER_ROLE);
    }

    modifier onlyWhenUnlocked() {
        require(!isLocked, "Field is locked");
        _;
    }

    modifier onlyWhenValidationUnlocked() {
        require(!isValidationLocked, "Validation field is locked");
        _;
    }

    modifier onlyOnce() {
        require(!isMessageAdded, "Message has already been added");
        _;
    }

    function addMessage(
        uint8 _targetType,
        int256[2] memory _groundCoordinates,
        uint256 _tleLine1,
        uint256 _tleLine2,
        uint256[2] memory _timeWindow,
        address _requesterId
    ) public onlyRole(PROPOSER_ROLE) onlyOnce {
        if (_targetType == 0) {
            message = Message({
                targetType: _targetType,
                groundCoordinates: _groundCoordinates,
                tleLine1: 0,
                tleLine2: 0,
                timeWindow: _timeWindow,
                requesterId: _requesterId,
                proposerId: msg.sender,
                data: new bytes(0),
                requesterValidation: false
            });
        } else {
            message = Message({
                targetType: _targetType,
                groundCoordinates: [int256(0), int256(0)],
                tleLine1: _tleLine1,
                tleLine2: _tleLine2,
                timeWindow: _timeWindow,
                requesterId: _requesterId,
                proposerId: msg.sender,
                data: new bytes(0),
                requesterValidation: false
            });
        }
        isMessageAdded = true;
        emit MessageAdded(_targetType, _requesterId);
    }

    function updateData(
        bytes memory _data
    ) public onlyRole(PROPOSER_ROLE) onlyWhenUnlocked {
        message.data = _data;
        isLocked = true; // Lock the data field after it is updated
        emit MessageLocked(); // Emit an event when the data field is locked
    }

    function updateRequesterValidation(
        bool validation
    ) public onlyRole(REQUESTER_ROLE) onlyWhenValidationUnlocked {
        require(isLocked, "Data is empty");
        message.requesterValidation = validation;
        isValidationLocked = true;
        emit ValidationLocked(); // Emit an event when the validation field is locked
    }

    // Retrieve message details
    function getMessage()
        public
        view
        returns (
            uint8 targetType,
            int256[2] memory groundCoordinates,
            uint256 tleLine1,
            uint256 tleLine2,
            uint256[2] memory timeWindow,
            address requesterId,
            address proposerId,
            bytes memory data,
            bool requesterValidation
        )
    {
        return (
            message.targetType,
            message.groundCoordinates,
            message.tleLine1,
            message.tleLine2,
            message.timeWindow,
            message.requesterId,
            message.proposerId,
            message.data,
            message.requesterValidation
        );
    }

    function getTargetType() public view returns (uint8) {
        return message.targetType;
    }

    function getGroundCoordinates() public view returns (int256[2] memory) {
        return message.groundCoordinates;
    }

    function getTleLine1() public view returns (uint256) {
        return message.tleLine1;
    }

    function getTleLine2() public view returns (uint256) {
        return message.tleLine2;
    }

    function getTimeWindow() public view returns (uint256[2] memory) {
        return message.timeWindow;
    }

    function getRequesterId() public view returns (address) {
        return message.requesterId;
    }

    function getProposerId() public view returns (address) {
        return message.proposerId;
    }

    function getData() public view returns (bytes memory) {
        return message.data;
    }

    function getRequesterValidation() public view returns (bool) {
        return message.requesterValidation;
    }
}
